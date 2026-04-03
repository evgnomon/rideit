
namespace rideit
{
    class MyClass
    {
        private int myField;

        public MyClass(int value)
        {
            myField = value;
        }
        public void DisplayValue()
        {
            Console.WriteLine($"The value is: {myField}");
        }
    }
}