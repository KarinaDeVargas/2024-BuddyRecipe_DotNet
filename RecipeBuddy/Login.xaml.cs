using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace RecipeBuddy
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public int userId { get; private set; }
        bool registration = false;
        public Login()
        {
            InitializeComponent();
            Globals.dbContext = new RecipeDBEntities();
        }
        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnLogin_Click(sender, e);
            }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginError.Visibility = Visibility.Hidden;
            try
            {
                user selUser = Globals.dbContext.users.FirstOrDefault(user => user.username == TbxUser.Text);

                if (selUser != null)
                {

                    if (BCrypt.Net.BCrypt.Verify(TbxPass.Password, selUser.password))
                    {
                        userId = selUser.userId;
                        this.DialogResult = true;
                    }
                }
                LoginError.Visibility = Visibility.Visible;
            }
            catch (NullReferenceException)
            {
                LoginError.Visibility = Visibility.Visible;
                return;
            }
            catch (SystemException ex)
            {
                MessageBox.Show(this, "System Error", ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRegi_Click(object sender, RoutedEventArgs e)
        {
            PassMatchValError.Visibility = Visibility.Hidden;
            PassLenValError.Visibility = Visibility.Hidden;
            if (registration == false)
            {
                LoginError.Visibility = Visibility.Hidden;
                BtnLogin.Visibility = Visibility.Hidden;
                LblAccount.Visibility = Visibility.Hidden;
                TbxPassValid.Visibility = Visibility.Visible;
                LblCnfPass.Visibility = Visibility.Visible;
                BtnRegiCnl.Visibility = Visibility.Visible;

                registration = true;
            }
            else
            {
                if (TbxPass.Password != TbxPassValid.Password)
                {
                    PassMatchValError.Visibility = Visibility.Visible;
                    return;
                } else if (TbxPass.Password.Length < 6 || !Regex.IsMatch(TbxPass.Password, @"^(?=.*[A-Za-z])(?=.*\d).+$"))
                {
                    PassLenValError.Visibility = Visibility.Visible;
                    return;
                }

                else
                {
                    try
                    {
                        user newUser = new user(TbxUser.Text, BCrypt.Net.BCrypt.HashPassword(TbxPass.Password));
                        Globals.dbContext.users.Add(newUser);
                        Globals.dbContext.SaveChanges();
                        user selUser = Globals.dbContext.users.FirstOrDefault(user => user.username == TbxUser.Text);
                        if (selUser != null)
                        {
                            userId = selUser.userId;
                            this.DialogResult = true;
                        }

                    }
                    catch (SystemException ex)
                    {
                        MessageBox.Show(this, "System Error", ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                }
            }
        }

        private void BtnRegiCnl_Click(object sender, RoutedEventArgs e)
        {
            registration = false;
            BtnLogin.Visibility = Visibility.Visible;
            LblAccount.Visibility = Visibility.Visible;
            PassLenValError.Visibility = Visibility.Hidden;
            PassMatchValError.Visibility = Visibility.Hidden;
            TbxPassValid.Visibility = Visibility.Hidden;
            LblCnfPass.Visibility = Visibility.Hidden;
            BtnRegiCnl.Visibility = Visibility.Hidden;
        }
    }
}
