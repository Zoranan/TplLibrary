using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tpl
{
    public class ConsoleColorWrapper
    {
        public void Write<T>(T msg)
        {
            Console.Write(msg);
        }

        public void Write<T>(T msg, ConsoleColor foregroundColor)
        {
            var t = Console.ForegroundColor;
            Console.ForegroundColor = foregroundColor;
            Write(msg);
            Console.ForegroundColor = t;
        }

        public void Write<T>(T msg, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
        {
            var t = Console.BackgroundColor;
            Console.BackgroundColor = backgroundColor;
            Write(msg, foregroundColor);
            Console.BackgroundColor = t;
        }

        //
        //
        //

        public void WriteLine()
        {
            Console.WriteLine();
        }

        public void WriteLine<T>(T msg)
        {
            Console.WriteLine(msg);
        }

        public void WriteLine<T>(T msg, ConsoleColor foregroundColor)
        {
            var t = Console.ForegroundColor;
            Console.ForegroundColor = foregroundColor;
            WriteLine(msg);
            Console.ForegroundColor = t;
        }

        public void WriteLine<T>(T msg, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
        {
            var t = Console.BackgroundColor;
            Console.BackgroundColor = backgroundColor;
            WriteLine(msg, foregroundColor);
            Console.BackgroundColor = t;
        }

        //
        // 
        // 
        
        public void Clear()
        {
            Console.Clear();
        }

        //
        //
        //

        public bool YesNoPrompt<T>(T prompt)
        {
            Write(prompt);
            var res = char.ToUpper(Console.ReadKey().KeyChar) == 'Y';
            WriteLine();
            return res;
        }

        public bool YesNoPrompt<T>(T prompt, ConsoleColor foregroundColor)
        {
            var t = Console.ForegroundColor;
            Console.ForegroundColor = foregroundColor;
            var res = YesNoPrompt(prompt);
            Console.ForegroundColor = t;
            return res;
        }

        public bool YesNoPrompt<T>(T prompt, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
        {
            var t = Console.BackgroundColor;
            Console.BackgroundColor = backgroundColor;
            var res = YesNoPrompt(prompt, foregroundColor);
            Console.BackgroundColor = t;
            return res;
        }

        public void WaitForEnter(string msg = "Press ENTER to continue")
        {
            if (msg != null)
            {
                Write(msg);
            }

            while (Console.ReadKey(true).Key != ConsoleKey.Enter) ;
        }
    }
}
