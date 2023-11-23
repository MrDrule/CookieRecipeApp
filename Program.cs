using CookieCookbook.Recipes;
using CookieCookbook.Recipes.Ingredients;
using System.Text.Json;

const FileFormat Format = FileFormat.Json;

IStringsRepository stringsRepository = Format == FileFormat.Json ? new StringsJsonRepository() : new StringsTextualRepository();
var ingredientsRegister = new IngredientRegister();
const string FileName = "recipes";
var fileMetadata = new FileMetadata(FileName, Format).ToPath();


var cookiesRecipeApp = new CookiesRecipesApp(new RecipesRepository(
    stringsRepository, ingredientsRegister),
    new RecipesConsoleUserInteraction(ingredientsRegister));
cookiesRecipeApp.Run(fileMetadata);

public class FileMetadata
{
    public FileMetadata(string name, FileFormat format)
    {
        Name = name;
        Format = format;
    }

    public string Name { get; }
    public FileFormat Format { get;}

    public string ToPath() => $"{Name}.{Format.AsFileExtension()}";
}
public static class FileFormatExtensions
{
    public static string AsFileExtension(this FileFormat format) => format == FileFormat.Json ? "json" : "txt";
}
public enum FileFormat
{
    Json,
    Txt
}
public class CookiesRecipesApp
{
    private readonly IRecipesRepository _recipesRepository;
    private readonly IRecipesUserInteraction _recipesUserInteraction;

    public CookiesRecipesApp(
        IRecipesRepository recipesRepository,
        IRecipesUserInteraction recipesUserInteraction)
    {
        _recipesRepository = recipesRepository;
        _recipesUserInteraction = recipesUserInteraction;
    }

    public void Run(string filePath)
    {
        var allRecipes = _recipesRepository.Read(filePath);
        _recipesUserInteraction.PrintExistingRecipes(allRecipes);

        _recipesUserInteraction.PromptToCreateRecipe();

        var ingredients = _recipesUserInteraction.ReadIngredientsFromUser();

        if (ingredients.Count() > 0)
        {
            var recipe = new Recipe(ingredients);
            allRecipes.Add(recipe);
            _recipesRepository.Write(filePath, allRecipes);

            _recipesUserInteraction.ShowMessage("Recipe added:");
            _recipesUserInteraction.ShowMessage(recipe.ToString());
        }
        else
        {
            _recipesUserInteraction.ShowMessage(
                "No ingredients have been selected. " +
                "Recipe will not be saved.");
        }

        _recipesUserInteraction.Exit();
    }
}
public interface IRecipesUserInteraction
{
    void ShowMessage(string message);
    void Exit();
    void PrintExistingRecipes(IEnumerable<Recipe> allRecipes);
    void PromptToCreateRecipe();
    IEnumerable<Ingredient> ReadIngredientsFromUser();
}

public interface IIngredientRegister
{
    IEnumerable<Ingredient> All { get; }

    Ingredient GetById(int id);
}

public class IngredientRegister : IIngredientRegister
{
    public IEnumerable<Ingredient> All { get; } = new List<Ingredient>
    {
        new WheatFlour(),
        new CoconutFlour(),
        new Butter(),
        new Chocolate(),
        new Sugar(),
        new Cardamom(),
        new Cinnamon(),
        new CocoaPowder()

    };

    public Ingredient GetById(int id)
    {
        foreach (var ingredient in All)
        {
            if (ingredient.Id == id) return ingredient;
        }
        return null;
    }

}
public class RecipesConsoleUserInteraction : IRecipesUserInteraction
{
    private readonly IIngredientRegister _ingredientRegister;

    public RecipesConsoleUserInteraction(IIngredientRegister ingredientRegister)
    {
        _ingredientRegister = ingredientRegister;
    }

    public void ShowMessage(string message)
    {
        Console.WriteLine(message);
    }

    public void Exit()
    {
        Console.WriteLine("Press any key to close.");
        Console.ReadKey();
    }
    public void PrintExistingRecipes(IEnumerable<Recipe> allRecipes)
    {
        if(allRecipes.Count() > 0)
        {
            Console.WriteLine("Existing recipes are:" + Environment.NewLine);
            var counter = 1;
            foreach(var recipe in allRecipes)
            {
                Console.WriteLine($"*****{counter}*****");
                Console.WriteLine(recipe);
                Console.WriteLine();
                counter++;
            }
        }
    }

    public void PromptToCreateRecipe()
    {

        Console.WriteLine("Create a new cookie recipe! Available ingredients are:");
        foreach(var ingredient in _ingredientRegister.All)
        {
            Console.WriteLine(ingredient);
        }
    }

    public IEnumerable<Ingredient> ReadIngredientsFromUser()
    {
        bool stop = false;
        var ingredients = new List<Ingredient>();

        while (!stop)
        {
            Console.WriteLine("Add an ingredient by its ID, or type anything else if finished!");
            var userInput = Console.ReadLine();

            if (int.TryParse(userInput, out int id))
            {
                var selectedIngredient = _ingredientRegister.GetById(id);
                if (selectedIngredient is not null)
                {
                    ingredients.Add(selectedIngredient);

                }
            }
            else
            {
                stop = true;
            }
        }
        return ingredients;
    }
}
public interface IRecipesRepository
{
    List<Recipe> Read(string filePath);
    void Write(string filePath, List<Recipe> allRecipes);
}

public class RecipesRepository : IRecipesRepository
{
    private readonly IStringsRepository _stringsRepository;
    private readonly IIngredientRegister _ingredientRegister;
    private const string Seperator = ",";
    public RecipesRepository(IStringsRepository stringsRepository, IIngredientRegister ingredientRegister)
    {
        _stringsRepository = stringsRepository;
        _ingredientRegister = ingredientRegister;
    }

    public List<Recipe> Read (string filePath)
    {
        List<string> recipesFromFile = _stringsRepository.Read(filePath);
        var recipes = new List<Recipe>();
        foreach (var recipeFromFile in recipesFromFile)
        {
            var recipe = RecipeFromString(recipeFromFile);
            recipes.Add(recipe);

        }
        return recipes;
    }

    private Recipe RecipeFromString(string recipeFromFile)
    {
        var textualIds = recipeFromFile.Split(Seperator);
        var ingredients = new List<Ingredient>();

        foreach(var textualId in textualIds)
        {
            var id = int.Parse(textualId);
            var ingredient =_ingredientRegister.GetById(id);
            ingredients.Add(ingredient);
        }
        return new Recipe(ingredients);
    }

    public void Write(string filePath, List<Recipe> allRecipes)
    {
        var recipesAsStrings = new List<string>();
        foreach (var recipe in allRecipes)
        {
            var allIds = new List<int>();
            foreach(var ingredient in recipe.Ingredients)
            {
                allIds.Add(ingredient.Id);
            }
            recipesAsStrings.Add(string.Join(Seperator, allIds));    
        }
        _stringsRepository.Write(filePath, recipesAsStrings);
    }
}
public interface IStringsRepository
{
    List<string> Read(string filePath);
    void Write(string filePath, List<string> strings);
}
public abstract class StringsRepository : IStringsRepository
{
   
    public List<string> Read(string filePath)
    {
        if (File.Exists(filePath))
        {
            var fileContents = File.ReadAllText(filePath);
            return TextToStrings(fileContents);
        }
        return new List<string>();

    }

    protected abstract List<string> TextToStrings(string fileContents);

    public void Write(string filePath, List<string> strings)
    {
        File.WriteAllText(filePath, StringsToText(strings));
    }

    protected abstract string StringsToText(List<string> strings);
}

public class StringsTextualRepository : StringsRepository
{
    private static readonly string Seperator = Environment.NewLine;

    
    protected override string StringsToText(List<string> strings)
    {
        return string.Join(Seperator, strings);
    }

    protected override List<string> TextToStrings(string fileContents)
    {
        return fileContents.Split(Seperator).ToList();
    }
}

public class StringsJsonRepository : StringsRepository
{


    protected override string StringsToText(List<string> strings)
    {
        return JsonSerializer.Serialize(strings);
    }

    protected override List<string> TextToStrings(string fileContents)
    {
        return JsonSerializer.Deserialize<List<string>>(fileContents);
    }
}