using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ItraTask3.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace ItraTask3.Controllers
{
    public class HomeController : Controller
    {
        UserManager<ApplicationUser> _userManager;
        SignInManager<ApplicationUser> _signInManager;


        public HomeController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public static byte[] LastKey;
        public static byte[] LastChoice;

        public IActionResult Index(string UserChoice)
        {
            if (_signInManager.IsSignedIn(User))
            {
                byte[] key = RandomBytes(16);
                uint ComputerChoice;
                byte[] ComputerChoiceByte;
                string[] ArrayEntities = GetArrayEntities();
                int leng = ArrayEntities.Count();
                do
                {
                    ComputerChoiceByte = RandomBytes(2);
                    ComputerChoice = BitConverter.ToUInt16(ComputerChoiceByte, 0);
                } while (ComputerChoice >= leng);
                ViewData["Entities"] = WINLOSE(GetArrayEntities());
                byte[] EntityNameByte;
                if (UserChoice != null)
                {
                    Play(UserChoice);
                }
                LastKey = key;
                LastChoice = ComputerChoiceByte;
                EntityNameByte = System.Text.Encoding.UTF8.GetBytes(GetArrayEntities()[ComputerChoice]);
                ViewData["HmacNext"] = BitConverter.ToString(ComputeHmacsha1(EntityNameByte, key)).Replace("-",String.Empty);
                return View("Index");
            }
            else return Redirect("Identity/Account/Login");
        }
        
        private void Play(string UserChoice)
        {
            int UserChoiceInt = Array.IndexOf(GetArrayEntities(), UserChoice);
            byte[] EntityNameByte;
            uint ComputerChoiceLast = BitConverter.ToUInt16(LastChoice, 0);
            EntityNameByte = System.Text.Encoding.UTF8.GetBytes(GetArrayEntities()[ComputerChoiceLast]);
            ViewData["HmacNow"] = BitConverter.ToString(ComputeHmacsha1(EntityNameByte, LastKey)).Replace("-", String.Empty);
            ViewData["Key"] = BitConverter.ToString(LastKey).Replace("-", String.Empty);
            ViewData["ComputerChoice"] = GetArrayEntities()[ComputerChoiceLast];
            if (ComputerChoiceLast == UserChoiceInt)
            {
                ViewData["Result"] = "Draw";
            }
            else
            if (IsFirstWin(Convert.ToInt32(ComputerChoiceLast), UserChoiceInt))
            {
                ViewData["Result"] = "You lose";
            }
            else
            {
                ViewData["Result"] = "You win";
            }
        }

        private string[] GetArrayEntities()
        {
            List<ApplicationUser> users = _userManager.Users.ToList();
            foreach (ApplicationUser user in users)
            {
                if (user != null)
                {
                    if (User.Identity.Name == user.Email)
                    {
                        string[] entity = user.Settings.Split(new char[] { ',' });
                        return entity;
                    }
                }
            }
            return null;
        }

        private static byte[] ComputeHmacsha1(byte[] data, byte[] key)
        {
            using (var hmac = new HMACSHA256(key))
            {
                return hmac.ComputeHash(data);
            }
        }

        private byte[] RandomBytes(int i)
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            byte[] bytes = new byte[i];
            provider.GetBytes(bytes);
            return bytes;
        }

        public IActionResult Settings()
        {
             List<Entity> entities = WINLOSE(GetArrayEntities());
             return View(entities);
        }

        private List<Entity> WINLOSE(string[] entity)
        {
            List<Entity> entities = new List<Entity>();
            foreach(string en in entity)
            {
                entities.Add(new Entity { Name = en, Lose = "", Win = "" });
            }
            
            for(int i=0;i< entities.Count();i++)
            {
                for (int j = 0; j < entities.Count(); j++)
                {
                    if (entities[i].Name != entities[j].Name)
                    {
                        if (IsFirstWin(i, j))
                        {
                            entities[i].Win = entities[i].Win + " " + entities[j].Name;
                            entities[j].Lose = entities[j].Lose + " " + entities[i].Name;
                        }
                        
                       
                    }
                }
            }

            return entities;
        }
       
        private bool IsFirstWin(int firstNumber, int secondNumber)
        {
            int difference = Math.Abs(firstNumber - secondNumber);
            if (firstNumber > secondNumber)
            {
                if (difference % 2 == 0)
                {
                    return true;
                }
                else return false;
            } else if (difference % 2 == 0)
            {
                return false;
            }
            else return true;

        }
        [HttpPost]
        public async Task<ActionResult> Add(string[] entity)
        {

            string stringToAdd = "";
            int j = GetArrayEntities().Count();
            for (int i=0; i< entity.Length;i++)
            {
                entity[i] = entity[i].Replace(',',' ');
               
                if (entity[i] == "" | entity[i] == "Entity name")
                {
                    stringToAdd = stringToAdd + ",Entity " + j.ToString() ;
                    j++;
                }
                else
                {
                    stringToAdd = stringToAdd + "," + entity[i];
                }
            }

            List<ApplicationUser> users = _userManager.Users.ToList();
            foreach (ApplicationUser user in users)
            {
                if (user != null)
                {
                    if (User.Identity.Name == user.Email)
                    {
                        user.Settings = user.Settings + stringToAdd;
                        await _userManager.UpdateAsync(user);
                    }
                }
            }

            return RedirectToAction("Settings");
        }

        [HttpPost]
        public async Task<ActionResult> Delete()
        {
            List<ApplicationUser> users = _userManager.Users.ToList();
            foreach (ApplicationUser user in users)
            {
                if (user != null)
                {
                    if (User.Identity.Name == user.Email)
                    {
                        string[] entity = user.Settings.Split(new char[] { ',' });
                        if (entity.Length > 3)
                        {
                            user.Settings = entity[0];
                            for (int i = 1; i < entity.Length - 2; i++)
                            {
                                user.Settings = user.Settings + "," + entity[i];
                            }

                            await _userManager.UpdateAsync(user);
                        }
                    }
                }
            }

            return RedirectToAction("Settings");
        }


        [HttpPost]
        public async Task<ActionResult> Save(string[] SaveEntity)
        {
            int j = GetArrayEntities().Count();
            string stringToAdd = "";
            for (int i = 0; i < SaveEntity.Length; i++)
            {
                SaveEntity[i] = SaveEntity[i].Replace(',', ' ');
                if (SaveEntity[i] == "" | SaveEntity[i] == "Entity name")
                {
                    stringToAdd = stringToAdd + ",Entity" + j.ToString();
                    j++;
                }
                else
                {
                    stringToAdd = stringToAdd + "," + SaveEntity[i];
                }
            }

            List<ApplicationUser> users = _userManager.Users.ToList();
            foreach (ApplicationUser user in users)
            {
                if (user != null)
                {
                    if (User.Identity.Name == user.Email)
                    {
                        user.Settings = stringToAdd.Substring(1);
                        await _userManager.UpdateAsync(user);
                    }
                }
            }

            return RedirectToAction("Settings");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
