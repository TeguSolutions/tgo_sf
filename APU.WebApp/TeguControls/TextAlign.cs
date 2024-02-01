// ReSharper disable once CheckNamespace
namespace Tegu.Blazor.Controls;

public class TextAlign
{
   public TextAlign(string name)
   {
      Name = name;
   }

   public string Name { get; }

   public static TextAlign Start { get; } = new("ts-ta-start");
   public static TextAlign Center { get; } = new("ts-ta-center");
   public static TextAlign End { get; } = new("ts-ta-end");
}