using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Toltech.App.Behaviors
{
    /// <summary>
    /// Comportement attaché WPF qui filtre la saisie d'un <see cref="TextBox"/>
    /// ou d'un contrôle <c>NumberBox</c> (qui contient un <see cref="TextBox"/> interne).
    /// </summary>
    public static class TextBoxInputFilterBehavior
    {
        /// <summary>
        /// Type de filtre appliqué à la saisie.
        /// </summary>
        public enum FilterType
        {
            /// <summary>Aucun filtrage.</summary>
            None,

            /// <summary>Nombres décimaux uniquement (séparateur : virgule).</summary>
            Numeric,

            /// <summary>Lettres et espaces uniquement.</summary>
            Alphabetic,

            /// <summary>Expression mathématique évaluée à la validation (Entrée ou perte de focus).</summary>
            Expression
        }

        private static readonly CultureInfo Fr = CultureInfo.GetCultureInfo("fr-FR");

        // Noms autorisés dans une expression.
        private static readonly string[] KnownNames =
            { "pi", "e", "ln", "log", "sqrt", "exp", "abs", "sin", "cos", "tan" };

        // ---------- Propriété attachée publique ----------
        public static readonly DependencyProperty InputFilterProperty =
            DependencyProperty.RegisterAttached(
                "InputFilter",
                typeof(FilterType),
                typeof(TextBoxInputFilterBehavior),
                new PropertyMetadata(FilterType.None, OnFilterChanged));

        public static FilterType GetInputFilter(DependencyObject obj) =>
            (FilterType)obj.GetValue(InputFilterProperty);

        public static void SetInputFilter(DependencyObject obj, FilterType value) =>
            obj.SetValue(InputFilterProperty, value);

        // ---------- Propriété privée posée sur le TextBox réel ----------
        private static readonly DependencyProperty EffectiveFilterProperty =
            DependencyProperty.RegisterAttached(
                "EffectiveFilter",
                typeof(FilterType),
                typeof(TextBoxInputFilterBehavior),
                new PropertyMetadata(FilterType.None));

        private static FilterType GetEffectiveFilter(DependencyObject obj) =>
            (FilterType)obj.GetValue(EffectiveFilterProperty);

        // ---------- Branchement ----------
        private static void OnFilterChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                ApplyFilter(textBox, (FilterType)e.NewValue);
                return;
            }

            if (d is Control control)
            {
                control.Loaded -= OnControlLoaded;
                control.Loaded += OnControlLoaded;

                if (control.IsLoaded)
                    ApplyFilterToNumberBox(control, (FilterType)e.NewValue);
            }
        }

        private static void OnControlLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is not Control control)
                return;

            ApplyFilterToNumberBox(control, GetInputFilter(control));
        }

        private static void ApplyFilterToNumberBox(Control control, FilterType filter)
        {
            control.ApplyTemplate();

            var textBox = FindVisualChild<TextBox>(control);

            if (textBox != null)
                ApplyFilter(textBox, filter);
        }

        private static T FindVisualChild<T>(DependencyObject parent)
            where T : DependencyObject
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T result)
                    return result;

                var descendant = FindVisualChild<T>(child);

                if (descendant != null)
                    return descendant;
            }

            return null;
        }

        private static void ApplyFilter(TextBox textBox, FilterType filter)
        {
            // Mémorise le filtre sur le TextBox réel (le NumberBox n'est pas le sender).
            textBox.SetValue(EffectiveFilterProperty, filter);

            // Évite les abonnements multiples.
            textBox.PreviewTextInput -= OnPreviewTextInput;
            textBox.PreviewKeyDown -= OnPreviewKeyDown;
            textBox.PreviewLostKeyboardFocus -= OnPreviewLostKeyboardFocus;
            DataObject.RemovePastingHandler(textBox, OnPaste);

            if (filter == FilterType.None)
                return;

            textBox.PreviewTextInput += OnPreviewTextInput;
            DataObject.AddPastingHandler(textBox, OnPaste);

            if (filter == FilterType.Expression)
            {
                textBox.PreviewKeyDown += OnPreviewKeyDown;
                textBox.PreviewLostKeyboardFocus += OnPreviewLostKeyboardFocus;
            }
        }

        // ---------- Évaluation à Entrée / perte de focus ----------
        private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox textBox)
                CommitExpression(textBox);
        }

        private static void OnPreviewLostKeyboardFocus(
            object sender,
            KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox textBox)
                CommitExpression(textBox);
        }

        private static readonly string[] FunctionNames =
            KnownNames.Where(n => n != "pi" && n != "e").ToArray();

        private static void CommitExpression(TextBox textBox)
        {
            string text = textBox.Text;

            if (string.IsNullOrWhiteSpace(text))
                return;

            string cleaned = SanitizeExpression(text);

            // Plus rien de valide : on vide le champ.
            if (string.IsNullOrWhiteSpace(cleaned))
            {
                textBox.Text = string.Empty;
                return;
            }

            if (!ExpressionEvaluator.TryEvaluate(cleaned, out double result))
            {
                // Dernier recours : ne garder que la partie numérique.
                string numeric = Regex.Replace(text, @"[^0-9,.\-]", "").Replace('.', ',');

                if (!double.TryParse(
                        numeric,
                        NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                        Fr,
                        out result))
                {
                    textBox.Text = string.Empty;
                    return;
                }
            }

            string formatted = result.ToString("G15", Fr);

            if (formatted == text)
                return;

            textBox.Text = formatted;
            textBox.CaretIndex = formatted.Length;
        }

        private static string SanitizeExpression(string text)
        {
            // 1. Caractères interdits.
            text = Regex.Replace(text, @"[^0-9a-zA-Z+\-*/^().,\s]", "");

            // 2. Lettres qui ne font pas partie d'une formule.
            string source = text;

            text = Regex.Replace(source, @"[a-zA-Z]+", m =>
            {
                string name = m.Value.ToLowerInvariant();

                if (name == "pi" || name == "e")
                    return m.Value;

                if (FunctionNames.Contains(name))
                {
                    int next = m.Index + m.Length;

                    while (next < source.Length && char.IsWhiteSpace(source[next]))
                        next++;

                    if (next < source.Length && source[next] == '(')
                        return m.Value;
                }

                return string.Empty;
            });

            // 3. Opérateurs orphelins.
            text = Regex.Replace(text, @"^[\s*/^,.]+", "");              // début
            text = Regex.Replace(text, @"[+\-*/^]+(\s*\))", "$1");       // avant ')'
            text = Regex.Replace(text, @"[\s+\-*/^,.]+$", "");           // fin

            // 4. Parenthèses : retire les ')' en trop, ferme les '(' manquantes.
            var sb = new System.Text.StringBuilder();
            int open = 0;

            foreach (char c in text)
            {
                if (c == '(')
                {
                    open++;
                }
                else if (c == ')')
                {
                    if (open == 0)
                        continue; // ')' sans '(' : supprimée
                    open--;
                }

                sb.Append(c);
            }

            sb.Append(')', open);

            return sb.ToString().Trim();
        }

        // ---------- Saisie ----------
        private static void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is not TextBox textBox)
                return;

            // Numérique : normalise le point en virgule.
            if (GetEffectiveFilter(textBox) == FilterType.Numeric && e.Text == ".")
            {
                e.Handled = true;
                ReplaceSelection(textBox, ",");
                return;
            }

            if (!IsInputAllowed(textBox, e.Text))
                e.Handled = true;
        }

        private static void ReplaceSelection(TextBox textBox, string value)
        {
            int start = textBox.SelectionStart;
            int length = textBox.SelectionLength;

            textBox.Text = textBox.Text
                .Remove(start, length)
                .Insert(start, value);

            textBox.SelectionStart = start + value.Length;
            textBox.SelectionLength = 0;
        }

        private static void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (sender is not TextBox textBox)
                return;

            string original =
                e.DataObject.GetData(DataFormats.UnicodeText) as string
                ?? e.DataObject.GetData(DataFormats.Text) as string;

            if (original == null)
            {
                e.CancelCommand();
                return;
            }

            string pasted = original;
            var filter = GetEffectiveFilter(textBox);

            if (filter == FilterType.Numeric)
                pasted = pasted.Replace('.', ',');

            if (!IsInputAllowed(textBox, pasted))
            {
                e.CancelCommand();
                return;
            }

            if (filter == FilterType.Numeric && pasted != original)
            {
                e.CancelCommand();
                ReplaceSelection(textBox, pasted);
            }
        }

        private static bool IsInputAllowed(TextBox textBox, string input)
        {
            return GetEffectiveFilter(textBox) switch
            {
                FilterType.Numeric => IsNumericInputValid(textBox, input),
                FilterType.Expression => IsExpressionInputValid(textBox, input),
                FilterType.Alphabetic => Regex.IsMatch(
                    input,
                    @"^[a-zA-Z\s]*$",
                    RegexOptions.None,
                    TimeSpan.FromMilliseconds(100)),
                _ => true
            };
        }

        private static string SimulateText(TextBox textBox, string input)
        {
            return textBox.Text
                .Remove(textBox.SelectionStart, textBox.SelectionLength)
                .Insert(textBox.SelectionStart, input);
        }

        private static bool IsNumericInputValid(TextBox textBox, string input)
        {
            const char separator = ',';

            string newText = SimulateText(textBox, input.Replace('.', ','));

            if (string.IsNullOrEmpty(newText) ||
                newText == "-" ||
                newText == separator.ToString() ||
                newText == "-" + separator)
            {
                return true;
            }

            return double.TryParse(
                newText,
                NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                Fr,
                out _);
        }

        private static bool IsExpressionInputValid(TextBox textBox, string input)
        {
            string newText = SimulateText(textBox, input);

            // Caractères autorisés.
            if (!Regex.IsMatch(newText, @"^[0-9a-zA-Z+\-*/^().,\s]*$",
                    RegexOptions.None, TimeSpan.FromMilliseconds(100)))
                return false;

            // Chaque suite de lettres doit rester le début d'un nom connu.
            foreach (Match m in Regex.Matches(newText, @"[a-zA-Z]+"))
            {
                if (!KnownNames.Any(n =>
                        n.StartsWith(m.Value, StringComparison.OrdinalIgnoreCase)))
                    return false;
            }

            return true;
        }

        // ---------- Évaluateur (descente récursive) ----------
        private sealed class ExpressionEvaluator
        {
            private readonly string _s;
            private int _pos;

            private ExpressionEvaluator(string s) => _s = s;

            public static bool TryEvaluate(string text, out double result)
            {
                result = 0;

                try
                {
                    var p = new ExpressionEvaluator(text);
                    double v = p.ParseExpression();
                    p.SkipSpaces();

                    if (p._pos != p._s.Length || double.IsNaN(v) || double.IsInfinity(v))
                        return false;

                    result = v;
                    return true;
                }
                catch (FormatException)
                {
                    return false;
                }
            }

            private void SkipSpaces()
            {
                while (_pos < _s.Length && char.IsWhiteSpace(_s[_pos]))
                    _pos++;
            }

            private bool Match(char c)
            {
                SkipSpaces();

                if (_pos < _s.Length && _s[_pos] == c)
                {
                    _pos++;
                    return true;
                }

                return false;
            }

            // expr = term (('+' | '-') term)*
            private double ParseExpression()
            {
                double v = ParseTerm();

                while (true)
                {
                    if (Match('+')) v += ParseTerm();
                    else if (Match('-')) v -= ParseTerm();
                    else return v;
                }
            }

            // term = power (('*' | '/' | implicite) power)*
            private double ParseTerm()
            {
                double v = ParsePower();

                while (true)
                {
                    if (Match('*')) v *= ParsePower();
                    else if (Match('/')) v /= ParsePower();
                    else if (StartsImplicitFactor()) v *= ParsePower(); // 2pi, 3(4+1)
                    else return v;
                }
            }

            private bool StartsImplicitFactor()
            {
                SkipSpaces();

                return _pos < _s.Length &&
                       (_s[_pos] == '(' || char.IsLetter(_s[_pos]));
            }

            // power = unary ('^' power)?   (associatif à droite)
            private double ParsePower()
            {
                double b = ParseUnary();

                if (Match('^'))
                    return Math.Pow(b, ParsePower());

                return b;
            }

            private double ParseUnary()
            {
                if (Match('-')) return -ParseUnary();
                if (Match('+')) return ParseUnary();

                return ParsePrimary();
            }

            private double ParsePrimary()
            {
                SkipSpaces();

                if (_pos >= _s.Length)
                    throw new FormatException();

                char c = _s[_pos];

                if (c == '(')
                {
                    _pos++;
                    double v = ParseExpression();

                    if (!Match(')'))
                        throw new FormatException();

                    return v;
                }

                if (char.IsDigit(c) || c == ',' || c == '.')
                    return ParseNumber();

                if (char.IsLetter(c))
                    return ParseNameOrFunction();

                throw new FormatException();
            }

            private double ParseNumber()
            {
                int start = _pos;

                while (_pos < _s.Length &&
                       (char.IsDigit(_s[_pos]) || _s[_pos] == ',' || _s[_pos] == '.'))
                    _pos++;

                string raw = _s.Substring(start, _pos - start).Replace(',', '.');

                if (!double.TryParse(raw, NumberStyles.AllowDecimalPoint,
                        CultureInfo.InvariantCulture, out double v))
                    throw new FormatException();

                return v;
            }

            private double ParseNameOrFunction()
            {
                int start = _pos;

                while (_pos < _s.Length && char.IsLetter(_s[_pos]))
                    _pos++;

                string name = _s.Substring(start, _pos - start).ToLowerInvariant();

                // Constantes.
                if (name == "pi") return Math.PI;
                if (name == "e") return Math.E;

                // Fonctions : nom(expr)
                if (!Match('('))
                    throw new FormatException();

                double arg = ParseExpression();

                if (!Match(')'))
                    throw new FormatException();

                return name switch
                {
                    "ln" => Math.Log(arg),
                    "log" => Math.Log10(arg),
                    "sqrt" => Math.Sqrt(arg),
                    "exp" => Math.Exp(arg),
                    "abs" => Math.Abs(arg),
                    "sin" => Math.Sin(arg),   // radians
                    "cos" => Math.Cos(arg),
                    "tan" => Math.Tan(arg),
                    _ => throw new FormatException()
                };
            }
        }
    }

}
