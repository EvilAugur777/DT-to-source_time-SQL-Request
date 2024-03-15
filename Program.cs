using System;
using System.Runtime.InteropServices;
using System.Globalization;


public class Program
{
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool OpenClipboard(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr SetClipboardData(uint format, IntPtr data);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool CloseClipboard();

    private static void DateInputError()
    {
        Console.WriteLine("Некорректный формат даты. Пожалуйста, введите дату в указанном формате.");
	}

    public static void Main()
    {
        bool exit = false;
        while (!exit)
        {

            Console.WriteLine("Допустимые форматы ввода:\n1970-01-01 01:01:01\n1970-01-01 01:01:01+00\n1970-01-01 01:01:01.123456\n1970-01-01 01:01:01.123456+00");
            Console.WriteLine("\nВвести начальный source_time:");

            string inputDateStart;
            string inputDateFinish;
            DateTimeOffset dto;
            bool isValidInput = false;

            do
            {
                inputDateStart = Console.ReadLine();

                string[] formats = { "yyyy-MM-dd HH:mm:sszz", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss.ffffffzz", "yyyy-MM-dd HH:mm:ss.ffffff", "yyyy-MM-dd HH:mm:ss.fffffffffzz", "yyyy-MM-dd HH:mm:ss.fffffffff" };
                CultureInfo provider = CultureInfo.InvariantCulture;

                isValidInput = DateTimeOffset.TryParseExact(inputDateStart, formats, provider, DateTimeStyles.None, out dto);

                if (!isValidInput)
				{ 
                    Program.DateInputError();
                }

            } while (!isValidInput);

            Console.WriteLine("Ввести конечный source_time:");

            do
            {
                inputDateFinish = Console.ReadLine();

                string[] formats = { "yyyy-MM-dd HH:mm:sszz", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss.ffffffzz", "yyyy-MM-dd HH:mm:ss.ffffff", "yyyy-MM-dd HH:mm:ss.fffffffffzz", "yyyy-MM-dd HH:mm:ss.fffffffff" };
                CultureInfo provider = CultureInfo.InvariantCulture;

                isValidInput = DateTimeOffset.TryParseExact(inputDateFinish, formats, provider, DateTimeStyles.None, out dto);

                if (!isValidInput) 
                {
                    Program.DateInputError();
                }

            } while (!isValidInput);

            DateTimeOffset dtoStart = DateTimeOffset.Parse(inputDateStart);
            long timestampStart = (long)(dtoStart.ToUnixTimeSeconds() + 11644473600) * 10000000;

            DateTimeOffset dtoFinish = DateTimeOffset.Parse(inputDateFinish);
            long timestampFinish = (long)(dtoFinish.ToUnixTimeSeconds() + 11644473600) * 10000000;

            string query = @"
Запрос:			
			
SELECT
  layer,
  archive_itemid,
  to_timestamp((data_raw.source_time / 10000000.0) - 11644473600) AS converted_timestamp,
  source_time,
  server_time,
  status_code,
  value
FROM
  data_raw
WHERE source_time >= " + timestampStart + " AND source_time <= " + timestampFinish +
"\nORDER BY converted_timestamp DESC";

            Console.WriteLine(query);

            link1:
            Console.WriteLine("\nНажмите C для копирования запроса в буфер обмена\nНажмите Enter, чтобы очистить консоль и продолжить, или 'Q' для выхода...");
            var key = Console.ReadKey(true);
            if(key.Key == ConsoleKey.Q)
            {
                exit = true;
            }
            else if (key.Key == ConsoleKey.C)
            {
                if (OpenClipboard(IntPtr.Zero))
                {
                  
                    IntPtr ptr = Marshal.StringToHGlobalUni(query);
                    SetClipboardData(13, ptr); // 13 - формат данных CF_UNICODETEXT
                    CloseClipboard();

                    Console.WriteLine("\nТекст запроса успешно скопирован в буфер обмена.");
                    Console.WriteLine("Нажмите Enter, чтобы очистить консоль и продолжить, или 'Q' для выхода...");
                    var keyC = Console.ReadKey(true);
                    if (keyC.Key == ConsoleKey.Q)
                    {
                        exit = true;
                    }
                    
                    else if (keyC.Key == ConsoleKey.Enter)
                    {
                                Console.Clear();
                    }
                    else
					{
                        Console.WriteLine("\nНекорректный ввод.");
                        goto link1;
                    }

                }
                else
                {
                    Console.WriteLine("Не удалось открыть буфер обмена, попробуйте запустить программу от имени администратора.");
                }
            }
            else if (key.Key == ConsoleKey.Enter)
            {
                Console.Clear();
            }
            else 
			{
                Console.WriteLine("\nНекорректный ввод.");
                goto link1;
            }
        }
    }
}