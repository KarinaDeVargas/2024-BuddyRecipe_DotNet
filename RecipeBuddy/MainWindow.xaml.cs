using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace RecipeBuddy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<recipe> Recipes = new List<recipe>();
        int userId;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                Globals.dbContext = new RecipeDBEntities();
            }
            catch (SystemException ex)
            {
                MessageBox.Show(this, "Error reading database\n" + ex.Message, "Fatal error", MessageBoxButton.OK, MessageBoxImage.Error);
                //Environment.Exit(1); maybe keep maybe delete
            }

            Login login = new Login();
            var logginResult = login.ShowDialog();
            if (logginResult != true)
            {
                Close();
            }
            else
            {
                userId = login.userId;
            }

            ListUserRecipes();

            //ListAllRecipes(); // Could be ListUserRecipes??
            Recipes = Globals.dbContext.recipes.ToList();
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(inputSearch.Text)) //verify if input is null
            {
                MessageBox.Show($"Please enter a recipe name first.");
            }
            else
            {
                string[] searchWords = inputSearch.Text.ToLower().Split(' '); //input transformed into lower case and split using space, each word belogs to the array

                foreach (var recipe in Recipes) // interaction 
                {
                    string recipeNameLower = recipe.recipeName.ToLower();

                    // Check if all search words are present in the recipe name
                    bool allWordsPresent = searchWords.All(word => recipeNameLower.Contains(word));

                    if (!allWordsPresent)
                    {
                        dgRecipes.Items.Remove(recipe);
                    }
                }
                dgRecipes.Items.Refresh();
                LblStatus.Text = "Showing results for " + inputSearch.Text;
            }
        }

        private void SearchMenu_Click(object sender, RoutedEventArgs e)
        {
            ListAllRecipes();
        }

        private void ClearSearch(object sender, RoutedEventArgs e)
        {
            dgRecipes.Items.Clear();
            foreach (var recipe in Recipes)
            {
                dgRecipes.Items.Add(recipe);
            }

            dgRecipes.Items.Refresh();

            LblStatus.Text = "Search cleared";
        }

        private void DataGridView(object sender, SelectionChangedEventArgs e) //_SelectionChanged
        {

        }

        private void MyRecipes_Click(object sender, RoutedEventArgs e)
        {
            ListUserRecipes();
        }

        private void ListUserRecipes()
        {
            dgRecipes.Items.Clear();
            List<recipe> userRecipes = new List<recipe>(Globals.dbContext.recipes.Where(recipe => recipe.userId == userId));
            foreach (var recipe in userRecipes)
            {
                dgRecipes.Items.Add(recipe);
            }
            dgRecipes.Items.Refresh();
            LblStatus.Text = "Showing your recipes";
        }

        private void ListAllRecipes()
        {
            dgRecipes.Items.Clear();
            List<recipe> allRecipes = new List<recipe>(Globals.dbContext.recipes.ToList());
            foreach (var recipe in allRecipes)
            {
                dgRecipes.Items.Add(recipe);
            }
            dgRecipes.Items.Refresh();
            LblStatus.Text = "Showing all recipes";
        }

        private void CreateNewRecipe_Click(object sender, RoutedEventArgs e)
        {
            CreateNewRecipe();
        }

        private void CreateNewRecipe()
        {
            CreateRecipeWindow createRecipeWindow = new CreateRecipeWindow(userId); // Pass the userId
            if (createRecipeWindow.ShowDialog() == true)
            {
                Recipes.Add(createRecipeWindow.NewRecipe);
                dgRecipes.Items.Refresh();
                LblStatus.Text = "Recipe created!";
                dgRecipes.Items.Refresh();
            }
        }

        private void OnRecipeRowSelected(object sender, RoutedEventArgs e)
        {
            var recipe = dgRecipes.SelectedItem as recipe;
            if (recipe != null){
                ViewRecipe(recipe);
            }
        }

        private void ViewRecipe(recipe recipe)
        {
            ViewRecipe viewRecipe = new ViewRecipe(recipe);
            if (viewRecipe.ShowDialog() == true)
            {
                //DEAD CODE? what is this?
                //Recipes.Add(viewRecipe.NewRecipe);
                //dgRecipes.Items.Refresh();
                //LblStatus.Text = "Recipe created!"; 
            }
            if (viewRecipe.DialogResult == false) { LblStatus.Text = viewRecipe.statusUpdate; }
            dgRecipes.Items.Refresh();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            dgRecipes.Items.Clear();

            userId = -1;
            LblStatus.Text = "Logged out";

            Login login = new Login();
            var logginResult = login.ShowDialog();
            if (logginResult != true)
            {
                this.Close();
            }
            else
            {
                userId = login.userId;
            }
        }
    }
}
