using System.Collections.Generic;

namespace MusicXML_to_TXT
{
  public interface IEventConverter
  {
    string SingleEventToTxt(EventInfo e, List<DurationsToChar> durations, EventInfo nextEvent = null);
    List<SongTxt> GetSongsListTxt(string inputDir, string inputTXTFileName);
  }
}