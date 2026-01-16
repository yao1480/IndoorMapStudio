using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SilenceLibraryForAE.MethodLibrary
{
    public sealed class CursorHelper
    {
        public static System.Windows.Forms.Cursor Get_CursorSteram(byte[] cursorFileBytes)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(cursorFileBytes))
                {
                    return new System.Windows.Forms.Cursor(ms as Stream);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
