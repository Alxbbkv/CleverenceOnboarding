using System.Text;

namespace Cleverence.StringCompressor;

/// <summary>
/// Решение Задачи 1: реализует алгоритмы сжатия и распаковки строки из маленьких латинских букв.
/// </summary>
public class StringCompressor
{
    /// <summary>
    /// Проверяет, является ли символ допустимым для компрессируемой входной строки
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    private bool IsValidChar(char c)
    {
        return char.IsAsciiLetterLower(c);
    }

    /// <summary>
    /// Выполняет компрессию входной строки. Например "abbccc" -> "ab2c3".
    /// </summary>
    /// <param name="inStr">Входная строка. Должна содержать только маленькие латинские буквы или быть пустой/null.</param>
    /// <returns>Если входная строка пустая или null -- возвращает пустую строку.</returns>
    /// <exception cref="ArgumentException">Вызывает ArgumentException, если во входной строке есть символы, отличные от маленьких латинских букв.</exception>
    public string Compress(string inStr)
    {
        if (string.IsNullOrEmpty(inStr))
        {
            return string.Empty;
        }
        
        var result = new StringBuilder();

        var position = -1;
        var actualChar = inStr[0];
        var actualCharCount = 0;

        while (position < inStr.Length)
        {
            position++;
            
            bool isSequenceEnded = (position == inStr.Length);
            
            var c = (!isSequenceEnded) ? inStr[position] : inStr[position-1];
            
            if (!IsValidChar(c))
            {
                var exceptionMsg = $"Input string contains invalid character at position {position}.";
                throw new ArgumentException(exceptionMsg);
            }
            
            isSequenceEnded |= (c != actualChar);

            if (isSequenceEnded)
            {
                result.Append(actualChar);
                
                if (actualCharCount > 1)
                {
                    result.Append(actualCharCount);
                }
                actualChar = c;
                actualCharCount = 1;
            }
            else
            {
                actualCharCount++;
            }
        }
        
        return result.ToString();
    }

    /// <summary>
    /// Выполняет декомпрессию строки по алгоритму из Т3. Например, "ab2c3d4" -> "abbcccdddd".
    /// </summary>
    /// <param name="inStr">Строка, упакованная по алгоритму из ТЗ. Например, "ab2c3d4"</param>
    /// <returns>Декомпрессированная строка. Например, "abbcccdddd". Если входная строка пустая или null -- возвращает пустую строку.</returns>
    /// <exception cref="ArgumentException">Исключение, если входная строка не соответствует тому, что выдает алгоритм компрессии.</exception>
    public string Decompress(string inStr)
    {
        if (string.IsNullOrEmpty(inStr))
        {
            return string.Empty;
        }

        if (!IsValidChar(inStr[0])) throw new ArgumentException($"Input string begins with invalid character.");

        var foundEntries = new List<(char, int)>();

        var predChar = ' ';
        var charCount = 0;
        var decimalOrder = 1;
        
        for (int i = inStr.Length - 1; i >= 0; i--)
        {
            var c = inStr[i];
            if (IsValidChar(c))
            {
                if (c == predChar) throw new ArgumentException($"Two identical characters next to each other in input string: {c}.");

                if (charCount == 0) charCount = 1;
                foundEntries.Add((c, charCount));
                charCount = 0;
                decimalOrder = 1;
                predChar = c;
            }
            else if (char.IsDigit(c))
            {
                charCount += DigitCharToInt(c) * decimalOrder;
                decimalOrder *= 10;
            }
            else
            {
                throw new ArgumentException($"Invalid character in input string: {c}.");
            }
        }

       
        var result = new StringBuilder();

        foundEntries.Reverse();
        foreach (var pair in foundEntries)
        {
            result.Append(new string(pair.Item1, pair.Item2));
        }
        
        return result.ToString();
    }

    /// <summary>
    /// Преобразует символы '0'..'9' в числа 0..9
    /// </summary>
    /// <param name="digit">символ '0'..'9'</param>
    /// <returns></returns>
    private int DigitCharToInt(char digit)
    {
        //return int.Parse(digit.ToString());
        
        const int asciiCodeFor0 = 48;
        return digit - asciiCodeFor0;
    }
}