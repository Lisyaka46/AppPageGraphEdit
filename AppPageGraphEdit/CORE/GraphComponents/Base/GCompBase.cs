using System;
using System.Collections.Generic;
using System.Text;

namespace AppPageGraphEdit.CORE.GraphComponents.Base
{
    internal abstract class GCompBase<T>
    {
        private T? _Value = default;
        /// <summary>
        /// Значение компонента<br/>
        /// При изменении значения вызывается событие ValueChanged, передающее новое значение
        /// </summary>
        protected virtual T? Value
        {
            get => _Value;
            set
            {
                _Value = value;
                ValueChanged?.Invoke(this, value);
            }
        }

        /// <summary>
        /// Событие, которое возникает при изменении значения компонента
        /// </summary>
        public event EventHandler<T?>? ValueChanged;
    }
}
