
namespace SkeletonsAdventure.HelperClasses
{
    internal interface ICloneableGameClass<T> where T : class
    {
        T Clone();
    }
}
