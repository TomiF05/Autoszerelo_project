namespace Shared
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty; // Ez maga a "belépőkártya"
        public string Role { get; set; } = string.Empty;  // Visszaküldjük, hogy Admin vagy User, hogy a Blazor tudja, mit mutasson
        public string Name { get; set; } = string.Empty;  // Hogy ki tudjuk írni: "Szia, Kovács János!"
    }
}