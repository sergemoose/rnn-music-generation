using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MusicXML_to_TXT
{
  public class CharBasedEventConverter : IEventConverter
  {
    public List<SongTxt> GetSongsListTxt(string inputDir, string inputTXTFileName)
    {
      // Input TXT
      var txt = File.ReadAllText(string.Concat(inputDir, inputTXTFileName));

      List<SongTxt> songsListTxt = new List<SongTxt>();

      var songsTxt = Regex.Split(txt, "\r\n\r\n").ToList();
      foreach (string songTxt in songsTxt)
      {
        SongTxt sng = new SongTxt();

        var measuresTxt = songTxt.Split('\n').ToList();
        foreach (string measureTxt in measuresTxt)
        {
          MeasureTxt msr = new MeasureTxt();
          msr.EventsTxt = measureTxt.Split(' ').Where(x => x != "" && x != "\r").ToList();
          sng.Measures.Add(msr);
        }

        songsListTxt.Add(sng);
      }
      return songsListTxt;
    }

    public string SingleEventToTxt(EventInfo e, List<DurationsToChar> durations, EventInfo nextEvent = null)
    {
      if (e.IsSongEnd || e.IsMeasureEnd)
      {
        return "\n";
      }

      if (e.IsRest)
      {
        return string.Concat(
          "R",
          durations.Where(x => x.Type == e.Type && x.IsDot == e.IsDot && x.TimeMod == e.TimeMod).FirstOrDefault().Char,
          " ");
      }

      return string.Concat(
        e.Alter == 0 ? e.PitchStep : e.PitchStep.ToLower(),
        e.Octave.ToString(),
        durations.Where(x => x.Type == e.Type && x.IsDot == e.IsDot && x.TimeMod == e.TimeMod).FirstOrDefault().Char,
        e.IsChord ? "+" : "",
        " ");
    }

  }
}
