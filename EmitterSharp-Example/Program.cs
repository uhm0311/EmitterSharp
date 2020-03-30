using System;
using System.Collections.Generic;
using System.Text;

namespace EmitterSharp.Example
{
    class Program
    {
        static readonly Random random = new Random();

        static int countOfEvent9001 = 0;
        static int countOfEvent42 = 0;
        static int countOfArray = 0;

        static void Main(string[] args)
        {
            ExampleEmitter emitter = new ExampleEmitter();
            emitter.On("9001", ItsOverNineThousands); // Count of event 9001 will be 1 and 3 in here.

            emitter.Once("9001", () => // Listener without argument.
            {
                // Count of event 9001 will be 2 in here.
                Print9001("What? nine thousands? There's no way that can be, right?");
            });

            emitter.Emit("9001", CreateRandomArray()); // Emit with argument.

            emitter.On("9001", (array) => // Listener with argument.
            {
                // Count of event 9001 will be 4, 5 and 6 in here.
                Print9001(ArrayToString(array));
            });

            emitter.Emit("9001", CreateRandomArray());

            emitter.On("42", (array) => // Listener with argument.
            {
                // Count of event 42 will be 1 and 3 in here.
                Print42(ArrayToString(array));
            });

            emitter.On("42", () => // Listener without argument.
            {
                // Count of event 42 will be 2 and 4 in here.
                Print42("Life, the universe and everything.");
            });

            emitter.Off("9001", ItsOverNineThousands);

            emitter.Emit("9001", CreateRandomArray()); // Emit with argument.
            emitter.Emit("42", CreateRandomArray()); // Emit with argument.
            emitter.Emit("42"); // Emit without argument.

            emitter.Off("42"); // Listeners for event 42 will not be called after here.
            emitter.Emit("42"); // Emit without argument.
            emitter.Emit("42", CreateRandomArray()); // Emit with argument.

            emitter.Emit("9001"); // Emit without argument.

            emitter.On("array", PrintArray); // Count of event array will be 1 and 2 in here.
            emitter.Emit("array", CreateRandomArray()); // Emit with argument.
            emitter.Emit("array"); // Emit without argument.

            emitter.Off(); // All listeners will not be called after here.
            emitter.Emit("42", CreateRandomArray()); // Emit with argument.
            emitter.Emit("9001"); // Emit without argument.
            emitter.Emit("array", CreateRandomArray()); // Emit with argument.

            Console.WriteLine("Press any key to continue...");
            Console.Read();
        }

        static int[] CreateRandomArray()
        {
            List<int> result = new List<int>();

            while (result.Count < 10)
            {
                result.Add(random.Next(0xff));
            }

            return result.ToArray();
        }

        static void ItsOverNineThousands()
        {
            Print9001("It's over nine thousands!!!");
        }

        static void Print9001(string message = "") 
        {
            Console.WriteLine("9001 ({0}) : {1}", ++countOfEvent9001, message);
        }

        static void Print42(string message = "")
        {
            Console.WriteLine("42 ({0}) : {1}", ++countOfEvent42, message);
        }

        static string ArrayToString(int[] array)
        {
            if (array != null)
            {
                StringBuilder builder = new StringBuilder();

                for (int i = 0; i < array.Length - 1; i++)
                {
                    builder.Append(array[i]).Append(", ");
                }

                return builder.Append(array[array.Length - 1]).ToString();
            }
            else
            {
                return "Array is null.";
            }
        }

        static void PrintArray(params int[] args)
        {
            Console.WriteLine("Array ({0}) : {1}", ++countOfArray, ArrayToString(args));
        }

        class ExampleEmitter : Emitter<ExampleEmitter, string, int[]>
        {

        }
    }
}
