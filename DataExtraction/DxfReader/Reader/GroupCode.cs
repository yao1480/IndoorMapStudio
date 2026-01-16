using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DXFReader.Reader
{
    /// <summary>
    /// 组码-组值
    /// </summary>
  public struct GroupCode
    {
      public string Code;
      public string Value;

      public GroupCode(string code,string value)
      {
          this.Code = code;
          this.Value = value;
      }

    }
}
