using AppPageGraphEdit.CORE.GraphComponents.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;

namespace AppPageGraphEdit.CORE.GraphComponents
{
    /// <summary>
    /// Компонент строкового значения
    /// </summary>
    internal class GCompString : GCompBase<string>
    {
        /// <summary>
        /// Визуальный элемент компонента, отображающий строковое значение<br/>
        /// </summary>
        public TextBlock Visual { get; }

        public GCompString()
        {
            Visual = new();
            ValueChanged += (sender, e) =>
            {
                Visual.Text = e ?? string.Empty;
            };
            Value = string.Empty;
        }
    }
}
