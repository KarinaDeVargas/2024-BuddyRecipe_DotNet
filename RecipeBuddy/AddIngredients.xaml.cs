using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RecipeBuddy
{
    /// <summary>
    /// Interaction logic for AddIngredients.xaml
    /// </summary>
    public partial class AddIngredients : Window
    {
        public ingredient addedIngredient { get; set; }
        public decimal quantity { get; set; }
        public string unit { get; set; }
        public FoodItem toAdd { get; set; }
        string appId = "c7f161dd";
        string appKey = "87bf2d1cd42db48aa4f80d55142480e4";
        public AddIngredients()
        {
            InitializeComponent();

        }

        private void LvIngredients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //toAdd = LvIngredients.SelectedItem as ingredient;
        }

        private async void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            //Call Api to get ingredient list
            string query = TbxSearch.Text;
            string baseUrl = $"https://api.edamam.com/api/food-database/v2/parser?app_id={appId}&app_key={appKey}&ingr={query}";

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage res = await client.GetAsync(baseUrl);

                if (res.IsSuccessStatusCode)
                {
                    string responseContent = await res.Content.ReadAsStringAsync();
                    EdamamResponse edamamResponse = JsonConvert.DeserializeObject<EdamamResponse>(responseContent);

                    // Extract relevant data into a list
                    List<FoodItem> foodItems = new List<FoodItem>();
                    foreach (Hints parsedFoodItem in edamamResponse.hints)
                    {
                        FoodItem foodItem = new FoodItem
                        {
                            ingredientName = parsedFoodItem.food.label,
                            carbs = (decimal)parsedFoodItem.food.nutrients.ENERC_KCAL,
                            fat = (decimal)parsedFoodItem.food.nutrients.FAT,
                            protein = (decimal)parsedFoodItem.food.nutrients.PROCNT
                        };
                        foodItems.Add(foodItem);
                    }
                    LvIngredients.ItemsSource = foodItems;
                }
                else
                {
                    Console.WriteLine($"Failed to fetch data. Status code: {res.StatusCode}");
                }
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            toAdd = LvIngredients.SelectedItem as FoodItem;

            var newIg = Globals.dbContext.ingredients.FirstOrDefault(ingredient => ingredient.ingredientName == toAdd.ingredientName);

            try
            {
                if (newIg == null)
                {
                    newIg = new ingredient
                    {
                        ingredientName = toAdd.ingredientName,
                        carbs = toAdd.carbs,
                        fat = toAdd.fat,
                        protein = toAdd.protein,

                    };
                    quantity = Decimal.Parse(TbxQuantity.Text);
                    unit = TbxUnits.Text;
                    addedIngredient = newIg;
                }
                else if (newIg != null)
                {
                    quantity = Decimal.Parse(TbxQuantity.Text);
                    unit = TbxUnits.Text;
                    addedIngredient = newIg;
                }
                this.DialogResult = true;
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(this, "Quantiy is invalid", ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }

    //Classes required for picking the Edamam JSON apart
    public class EdamamResponse
    {
        public List<Hints> hints { get; set; }
    }

    public class Hints
    {
        public FoodItemInfo food { get; set; }
    }

    public class FoodItemInfo
    {
        public string label { get; set; }
        public FoodNutrients nutrients { get; set; }
    }

    public class FoodNutrients
    {
        public double ENERC_KCAL { get; set; }

        public double PROCNT { get; set; }

        public double FAT { get; set; }
    }

    public class FoodItem
    {
        public string ingredientName { get; set; }
        public decimal fat { get; set; }

        public decimal carbs { get; set; }

        public decimal protein { get; set; }
    }
}
