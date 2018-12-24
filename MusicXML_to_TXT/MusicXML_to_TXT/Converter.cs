using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace MusicXML_to_TXT
{


  class Converter
  {
    //settings
    //---------------------------------------------------
    public static string inputXMLPath = "XML";
    public static string outputTXTPath = "TXT";
    public static string ouputTXTFileName = @"\output.txt";
    public static string inputTXTPath = "GeneratedTXT";
    public static string inputTXTFileName = @"\input.txt";
    public static string outputXMLPath = "GeneratedXML";
    public static string durationsFileName = @"\durations.json";
    public static string templatePath = "Template";
    public static string templateFileName = @"\template.xml";
    //---------------------------------------------------

    static void ConvertXML_To_Txt(IEventConverter converter)
    {

      string inputDir = Path.Combine(System.Environment.CurrentDirectory, inputXMLPath);
      string outputDir = Path.Combine(System.Environment.CurrentDirectory, outputTXTPath);

      string[] files = Directory.GetFiles(inputDir, "*.xml", SearchOption.AllDirectories);

      List<EventInfo> allEvents = new List<EventInfo>();

      // Собираем все события в одну структуру
      foreach (string file in files)
      {
        var doc = XDocument.Load(file);

        var measures = doc.Root.Element("part").Elements("measure").ToList();

        foreach (var m in measures)
        {
          var notes = m.Elements("note").ToList().Select(x =>
            new EventInfo()
            {
              PitchStep = (x.Element("pitch") != null) ? x.Element("pitch").Element("step").Value : null,
              Alter = (x.Element("pitch")?.Element("alter") != null) ? int.Parse(x.Element("pitch").Element("alter").Value) : 0,
              Octave = (x.Element("pitch") != null) ? int.Parse(x.Element("pitch").Element("octave").Value) : 0,
              Type = x.Element("type").Value,
              IsChord = x.Element("chord") != null,
              IsRest = x.Element("rest") != null,
              IsDot = x.Element("dot") != null,
              TimeMod = (x.Element("time-modification") != null) ?
                (int.Parse(x.Element("time-modification").Element("actual-notes").Value), int.Parse(x.Element("time-modification").Element("normal-notes").Value)) : (0, 0),
              Notehead = (x.Element("notehead") != null) ? x.Element("notehead").Value : null
            }).ToList();

          allEvents.AddRange(notes);
          // Конец такта
          allEvents.Add(new EventInfo() { IsMeasureEnd = true });

        }

        // Конец песни
        allEvents.Add(new EventInfo() { IsSongEnd = true });
      }

      // уникальные длительности и их соответствие символам
      var durations = allEvents.Where(x => x.Type != null).Select(x => new { x.Type, x.IsDot, x.TimeMod })
        .Distinct().Select((item, index) => new DurationsToChar
        {
          Char = (char)(255 - index),
          Type = item.Type,
          IsDot = item.IsDot,
          TimeMod = item.TimeMod
        }).ToList();

      JsonSerialization.WriteToJsonFile<List<DurationsToChar>>(string.Concat(outputDir, durationsFileName), durations);

      StringBuilder sb = new StringBuilder();


      for (int i = 0; i < allEvents.Count - 1; i++)
      {
        sb.Append(converter.SingleEventToTxt(allEvents[i], durations,  (i < allEvents.Count - 2) ? allEvents[i + 1] : null ));        
      }
        
      using (System.IO.StreamWriter file = new System.IO.StreamWriter(string.Concat(outputDir, ouputTXTFileName)))
      {
        file.WriteLine(sb.ToString());
      }
    }

    static void ConvertTxt_To_XML(IEventConverter converter)
    {
      string inputDir = Path.Combine(System.Environment.CurrentDirectory, inputTXTPath);
      string outputDir = Path.Combine(System.Environment.CurrentDirectory, outputXMLPath);
      string templateFilePath = Path.Combine(System.Environment.CurrentDirectory, templatePath);

      // Durations
      List<DurationsToChar> durations = JsonSerialization.ReadFromJsonFile<List<DurationsToChar>>(string.Concat(inputDir, durationsFileName));

      // Structure of events in TXT
      List<SongTxt> songsListTxt = converter.GetSongsListTxt(inputDir, inputTXTFileName);

      // XML Template
      var docTemplate = XDocument.Load(string.Concat(templateFilePath, templateFileName));

      int xmlNum = 1;

      foreach (var song in songsListTxt)
      {
        var doc = new XDocument(docTemplate);
        var partElement = doc.Root.Element("part");

        int measureNum = 1;
        foreach (var measure in song.Measures)
        {
          var measureElement = new XElement("measure");
          measureElement.Add(new XAttribute("number", measureNum.ToString()));

          foreach (string e in measure.EventsTxt)
          {

            try
            {

              var noteElement = new XElement("note");

              bool isRest = false;
              bool isChord = false;
              char durationChar;
              char pitchStep = ' ';
              int octave = 0;

              if (e[0] == 'R')
              {
                isRest = true;
                durationChar = e[1];
              }
              else
              {
                pitchStep = e[0];
                octave = (int)char.GetNumericValue(e[1]);
                durationChar = e[2];
                if (e.Length > 3 && e[3] == '+')
                  isChord = true;
              }

              DurationsToChar durationInfo = durations.Single(x => x.Char == durationChar);

              var pitchElement = new XElement("note");

              if (isRest)
              {
                noteElement.Add(new XElement("rest", ""));
              }
              else
              {
                noteElement.Add(
                  new XElement("pitch",
                    new XElement("step", pitchStep.ToString().ToUpper()),
                    new XElement("octave", octave),
                    char.IsLower(pitchStep) ? new XElement("alter", 1) : null
                  ));

                if (isChord)
                  noteElement.Add(new XElement("chord", ""));
              }

              // duration
              noteElement.Add(new XElement("type", durationInfo.Type));
              if (durationInfo.IsDot)
                noteElement.Add(new XElement("dot", ""));

              if (durationInfo.TimeMod != (0, 0))
                noteElement.Add(
                  new XElement("time-modification",
                    new XElement("actual-notes", durationInfo.TimeMod.actual),
                    new XElement("normal-notes", durationInfo.TimeMod.normal)
                  ));

              measureElement.Add(noteElement);

            }
            catch (Exception ex)
            {
              Console.WriteLine(ex.Message);
            }
          }

          partElement.Add(measureElement);
          measureNum++;
        }

        //save song xml
        doc.Save(string.Concat(outputDir, $@"\{xmlNum}.xml")); 

        xmlNum++;
      }


    }


    static void Main(string[] args)
    {
      //IEventConverter converter = new CharBasedEventConverter();
      IEventConverter converter = new WordBasedEventConverter();

      if (args.Length == 0)
        ConvertXML_To_Txt(converter);
      else
        ConvertTxt_To_XML(converter);


      //Console.ReadKey();


    }
  }
}
