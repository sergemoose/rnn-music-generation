using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicXML_to_TXT
{
  public class EventInfo
  {
    public string PitchStep;
    public int Alter;
    public int Octave;
    public string Type;
    public bool IsChord = false;
    public bool IsRest = false;
    public bool IsDot = false;
    public (int actual, int normal) TimeMod;
    public string Notehead;
    public bool IsMeasureEnd = false;
    public bool IsSongEnd = false;
  }

  public class DurationsToChar
  {
    public string Type;
    public bool IsDot = false;
    public (int actual, int normal) TimeMod;
    public char Char;

    public DurationsToChar() { }
  }

  public class MeasureTxt
  {
    public List<string> EventsTxt;

    public MeasureTxt()
    {
      EventsTxt = new List<string>();
    }
  }

  public class SongTxt
  {
    public List<MeasureTxt> Measures;

    public SongTxt()
    {
      Measures = new List<MeasureTxt>();
    }
  }
}
