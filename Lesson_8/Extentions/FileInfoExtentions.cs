using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lesson_8.Extention
{
    internal static class FileInfoExtensions
    {
        public static Process? Execute(this FileInfo file)
        {
            var processStartInfo = new ProcessStartInfo(file.FullName)
            {
                UseShellExecute = true
            };
            return Process.Start(processStartInfo);
        }
    }
}
