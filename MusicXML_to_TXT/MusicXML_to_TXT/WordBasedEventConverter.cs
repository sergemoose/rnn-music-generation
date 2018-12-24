using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MusicXML_to_TXT
{
  class WordBasedEventConverter : IEventConverter
  {
    public List<SongTxt> GetSongsListTxt(string inputDir, string inputTXTFileName)
    {
      // Input TXT
      var txt = File.ReadAllText(string.Concat(inputDir, inputTXTFileName));
      txt = txt.Replace("\r\n", "");

      List<SongTxt> songsListTxt = new List<SongTxt>();

      var songsTxt = Regex.Split(txt, "SongEnd").ToList();
      foreach (string songTxt in songsTxt)
      {
        SongTxt sng = new SongTxt();

        if (songTxt[0] != '\r')
        {
          var measuresTxt = Regex.Split(songTxt, "MeasureEnd").ToList();
          foreach (string measureTxt in measuresTxt)
          {
            if (measureTxt.Length > 0 && measureTxt[0] != '\r')
            {
              var notes = Regex.Split(measureTxt, @"(?=[+,' '])").ToList();
              notes.RemoveAll(x => (x == "" || x == " "));

              MeasureTxt msr = new MeasureTxt();
              msr.EventsTxt = notes.Select(x => x.Replace(" ", "")).Select(x => (x[0] == '+') ? x.Remove(0, 1) + "+" : x).ToList();
              sng.Measures.Add(msr);
            }
          }

          songsListTxt.Add(sng);
        }
      }
      return songsListTxt;
    }

    public string SingleEventToTxt(EventInfo e, List<DurationsToChar> durations, EventInfo nextEvent = null)
    {
      if (e.IsSongEnd)
      {
        return "SongEnd ";
      }

      if (e.IsSongEnd || e.IsMeasureEnd)
      {
        return "MeasureEnd ";
      }

      if (e.IsRest)
      {
        return string.Concat(
          "R",
          durations.Where(x => x.Type == e.Type && x.IsDot == e.IsDot && x.TimeMod == e.TimeMod).FirstOrDefault().Char,
          " ");
      }

      return string.Concat(
        e.IsChord ? "+" : "",
        e.Alter == 0 ? e.PitchStep : e.PitchStep.ToLower(),
        e.Octave.ToString(),
        durations.Where(x => x.Type == e.Type && x.IsDot == e.IsDot && x.TimeMod == e.TimeMod).FirstOrDefault().Char,
        nextEvent?.IsChord ?? false ? "" : " ");
    }
  }
}
