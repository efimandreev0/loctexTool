using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace loctext_Tool
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args[0].Contains(".txt"))
            {
                Rebuild(args[1], args[0]);
            }
            else
            {
                Extract(args[0]);
            }
        }
        public static void Extract(string loctext)
        {
            var reader = new BinaryReader(File.OpenRead(loctext));
            reader.BaseStream.Position = 0x0C;
            int count1 = reader.ReadInt32();
            int[] pointers1 = new int[count1];
            List<int> pointers = new List<int>();
            reader.BaseStream.Position += 0x08;
            int Block1Size = reader.ReadInt32();
            int Block1End = reader.ReadInt32();
            int fileSize = reader.ReadInt32();
            reader.BaseStream.Position += 0x08;
            for (int i = 0; i < count1; i++)
            {
                pointers1[i] = reader.ReadInt32();
            }
            reader.BaseStream.Position = Block1End;
            int textBlockSt = reader.ReadInt32();
            int l = 0;
            while (reader.BaseStream.Position != textBlockSt)
            {
                if(l == 0)
                {
                    pointers.Add(textBlockSt);
                }
                pointers.Add(reader.ReadInt32());
                l++;
            }
            string[] strings = new string[pointers.Count];
            for (int i = 0; i < count1; i++)
            {
                reader.BaseStream.Position = pointers[i];
                strings[i] = Utils.ReadString(reader, Encoding.UTF8);
            }
            File.WriteAllLines(loctext + ".txt", strings);
        }
        public static void Rebuild(string loc, string text)
        {
            var reader = new BinaryReader(File.OpenRead(loc));
            reader.BaseStream.Position = 0x1C;
            int nextBlock = reader.ReadInt32();
            int size = reader.ReadInt32();
            reader.BaseStream.Position = nextBlock;
            int textBlock = reader.ReadInt32();
            reader.Close();

            var writer = new BinaryWriter(File.OpenWrite(loc));
            string[] strings = File.ReadAllLines(text);
            int[] pointers = new int[strings.Length];
            writer.BaseStream.Position = 0x0C;
            writer.Write(strings.Length);
            writer.BaseStream.Position = textBlock;
            for (int i = 0; i < strings.Length; i++)
            {
                pointers[i] = (int)writer.BaseStream.Position;
                writer.Write(Encoding.UTF8.GetBytes(strings[i]));
                writer.Write(new byte());
            }
            writer.BaseStream.Position = nextBlock;
            for (int i = 0; i < strings.Length; i++)
            {
                writer.Write(pointers[i]);
            }
            writer.BaseStream.Position = 0x20;
            writer.Write(writer.BaseStream.Length);
        }
    }
}
