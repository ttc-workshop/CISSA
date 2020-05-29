using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;

namespace Intersoft.CISSA.UserApp.Models
{ 

    #region Models

    public class ChangePasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "CurrentPassword", ResourceType = typeof(Resources.Account) /*"Текущий пароль"*/)]
        public string OldPassword { get; set; }

        [Required]
        [ValidatePasswordLength]
        [DataType(DataType.Password)]
        [Display(Name = "NewPassword", ResourceType = typeof(Resources.Account) /*"Новый пароль"*/)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "ValidNewPassword", ResourceType = typeof(Resources.Account) /*"Подтвердите ввод нового пароля"*/)]
//        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "Новый пароль и пароль для подтверждения не совпадают")]
        public string ConfirmPassword { get; set; }
    }

    public class LogOnModel
    {
        [Required]
        [Display(Name = "UserName", ResourceType = typeof(Resources.Account) /*"Имя пользователя"*/)]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "UserPassword", ResourceType = typeof(Resources.Account) /*"Пароль"*/)]
        public string Password { get; set; }

        [Display(Name = "RememberMe", ResourceType = typeof(Resources.Account) /*"Запомнить меня?"*/)]
        public bool RememberMe { get; set; }
    }


    public class RegisterModel
    {
        [Required]
        [Display(Name = "UserName", ResourceType = typeof(Resources.Account) /*"Имя пользователя"*/)]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email", ResourceType = typeof(Resources.Account) /*"Электронная почта"*/)]
        public string Email { get; set; }

        [Required]
        [ValidatePasswordLength]
        [DataType(DataType.Password)]
        [Display(Name = "UserPassword", ResourceType = typeof(Resources.Account) /*"Пароль"*/)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "ValidPassword", ResourceType = typeof(Resources.Account) /*"Подтверждение пароля"*/)]
//        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "Новый пароль и пароль для подтверждения не совпадают.")]
        public string ConfirmPassword { get; set; }
    }
    #endregion

    #region Services
    // The FormsAuthentication type is sealed and contains static members, so it is difficult to
    // unit test code that calls its members. The interface and helper class below demonstrate
    // how to create an abstract wrapper around such a type in order to make the AccountController
    // code unit testable.

    public interface IMembershipService
    {
        int MinPasswordLength { get; }

        bool ValidateUser(string userName, string password);
        MembershipCreateStatus CreateUser(string userName, string password, string email);
        bool ChangePassword(string userName, string oldPassword, string newPassword);
    }

    public class AccountMembershipService : IMembershipService
    {
        private readonly MembershipProvider _provider;

        public AccountMembershipService()
            : this(null)
        {
        }

        public AccountMembershipService(MembershipProvider provider)
        {
            _provider = provider ?? new CissaMembershipProvider(); //Membership.Provider;
        }

        public int MinPasswordLength
        {
            get
            {
                return _provider.MinRequiredPasswordLength;
            }
        }

        public bool ValidateUser(string userName, string password)
        {
            if (String.IsNullOrEmpty(userName)) 
                throw new ArgumentException(Resources.Account.NotEmptyFieldValidation /*"Поле не может быть пустым."*/, "userName");
            if (String.IsNullOrEmpty(password)) 
                throw new ArgumentException(Resources.Account.NotEmptyFieldValidation /*"Поле не может быть пустым."*/, "password");

            return _provider.ValidateUser(userName, password);
        }

        public MembershipCreateStatus CreateUser(string userName, string password, string email)
        {
            if (String.IsNullOrEmpty(userName)) 
                throw new ArgumentException(Resources.Account.NotEmptyFieldValidation /*"Поле не может быть пустым."*/, "userName");
            if (String.IsNullOrEmpty(password)) 
                throw new ArgumentException(Resources.Account.NotEmptyFieldValidation /*"Поле не может быть пустым."*/, "password");
            if (String.IsNullOrEmpty(email)) 
                throw new ArgumentException(Resources.Account.NotEmptyFieldValidation /*"Поле не может быть пустым."*/, "email");

            MembershipCreateStatus status;
            _provider.CreateUser(userName, password, email, null, null, true, null, out status);
            return status;
        }

        public bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            if (String.IsNullOrEmpty(userName)) 
                throw new ArgumentException(Resources.Account.NotEmptyFieldValidation /*"Поле не может быть пустым."*/, "userName");
            if (String.IsNullOrEmpty(oldPassword)) 
                throw new ArgumentException(Resources.Account.NotEmptyFieldValidation /*"Поле не может быть пустым."*/, "oldPassword");
            if (String.IsNullOrEmpty(newPassword)) 
                throw new ArgumentException(Resources.Account.NotEmptyFieldValidation /*"Поле не может быть пустым."*/, "newPassword");

            // The underlying ChangePassword() will throw an exception rather
            // than return false in certain failure scenarios.
            try
            {
                MembershipUser currentUser = _provider.GetUser(userName, true /* userIsOnline */);
                return currentUser.ChangePassword(oldPassword, newPassword);
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (MembershipPasswordException)
            {
                return false;
            }
        }
    }

    public interface IFormsAuthenticationService
    {
        void SignIn(string userName, bool createPersistentCookie);
        void SignOut();
    }

    public class FormsAuthenticationService : IFormsAuthenticationService
    {
        public void SignIn(string userName, bool createPersistentCookie)
        {
            if (String.IsNullOrEmpty(userName)) 
                throw new ArgumentException(Resources.Account.NotEmptyFieldValidation /*"Поле не может быть пустым*/ + @"..", "userName");

            FormsAuthentication.SetAuthCookie(userName, createPersistentCookie);
        }

        public void SignOut()
        {
            FormsAuthentication.SignOut();
        }
    }
    #endregion

    #region Validation
    public static class AccountValidation
    {
        public static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "Пользователь с таким именем уже существует. Пожалуйста, введите другое.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "Пользователь с таким электронным адресом уже существует. Пожалуйста введите другой электронный адрес.";

                case MembershipCreateStatus.InvalidPassword:
                    return "Пароль неверен. Повторите ввод пароля еще раз.";

                case MembershipCreateStatus.InvalidEmail:
                    return "Электронный адрес неверен. Повторите ввод электронного адреса еще раз.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "Ответ на вопрос для восстановления пароля неправилен. Проверьте и повторите ввод еще раз.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "Вопрос восстановления пароля введен неверно. Пожалуйста проверьте и повторите ввод еще раз.";

                case MembershipCreateStatus.InvalidUserName:
                    return "Имя пользователя введено неверно. Пожалуйста проверьте и повторите ввод еще раз.";

                case MembershipCreateStatus.ProviderError:
                    return "Модуль аутентификации вернул ошибку. Пожалуйста проверьте введенные данные и повторите ввод еще раз. Если ошибка повтормиться, обратитесь к системному администратору.";

                case MembershipCreateStatus.UserRejected:
                    return "Запрос пользователя отменен. Пожалуйста проверьте введенные данные и повторите ввод еще раз. Если ошибка повтормиться, обратитесь к системному администратору.";

                default:
                    return "Неизвестная ошибка. Пожалуйста проверьте введенные данные и повторите ввод еще раз. Если ошибка повтормиться, обратитесь к системному администратору.";
            }
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ValidatePasswordLengthAttribute : ValidationAttribute, IClientValidatable
    {
        private const string _defaultErrorMessage = "'{0}' должно быть не менее {1} символов.";
        private readonly int _minCharacters = Membership.Provider.MinRequiredPasswordLength;

        public ValidatePasswordLengthAttribute()
            : base(_defaultErrorMessage)
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture, ErrorMessageString,
                name, _minCharacters);
        }

        public override bool IsValid(object value)
        {
            string valueAsString = value as string;
            return (valueAsString != null && valueAsString.Length >= _minCharacters);
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            return new[]{
                new ModelClientValidationStringLengthRule(FormatErrorMessage(metadata.GetDisplayName()), _minCharacters, int.MaxValue)
            };
        }
    }
    #endregion

}
