using Sirenix.OdinInspector;

namespace Sirenix.OdinInspector.Custom
{
    public interface IOdinFacade
    {
        object UntypedTarget { get; set; }
    }

    public interface IOdinFacade<T> : IOdinFacade
    {
        T Target { get; set; }
    }

    [HideLabel, HideReferenceObjectPicker, InlineProperty]
    public class OdinFacade<T> : IOdinFacade<T>
    {
        public T Target { get; set; }

        public object UntypedTarget
        {
            get => Target;
            set => Target = (T)value;
        }
    }

}