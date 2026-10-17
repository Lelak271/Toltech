using System.Linq.Expressions;
using System.Reflection;

namespace Toltech.App.ViewModels
{
    /// <summary>Accès lecture/écriture à une propriété d'un modèle, indépendant du type du modèle.</summary>
    public interface IPropertyAdapter<TValue>
    {
        string PropertyName { get; }
        TValue Value { get; set; }
    }

    /// <summary>
    /// Adaptateur générique sur le type du modèle (ModelData, Requirements...).
    /// Construit à partir d'une expression de propriété simple : m => m.Name.
    /// </summary>
    public sealed class PropertyAdapter<TModel, TValue> : IPropertyAdapter<TValue>
        where TModel : class
    {
        private readonly TModel _model;
        private readonly Func<TModel, TValue> _getter;
        private readonly Action<TModel, TValue> _setter;

        public string PropertyName { get; }

        public PropertyAdapter(TModel model, Expression<Func<TModel, TValue>> property)
        {
            _model = model;

            // Accepte m => m.Prop ainsi que m => (T)m.Prop (conversion implicite)
            var member = property.Body as MemberExpression
                         ?? (property.Body as UnaryExpression)?.Operand as MemberExpression;

            if (member?.Member is not PropertyInfo propertyInfo)
                throw new ArgumentException(
                    "L'expression doit désigner directement une propriété (ex : m => m.Name).",
                    nameof(property));

            PropertyName = propertyInfo.Name;
            _getter = property.Compile();

            var setMethod = propertyInfo.GetSetMethod()
                            ?? throw new ArgumentException(
                                $"La propriété '{PropertyName}' n'a pas de setter public.",
                                nameof(property));

            _setter = (Action<TModel, TValue>)Delegate.CreateDelegate(
                typeof(Action<TModel, TValue>), setMethod);
        }

        public TValue Value
        {
            get => _getter(_model);
            set => _setter(_model, value);
        }
    }
}