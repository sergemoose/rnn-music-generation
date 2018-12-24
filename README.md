# rnn-music-generation

A simple project to generate some MIDI riffs using LSTM neural network

### Description

For training data, I chose 30 MIDI songs by Slayer. From each one of those I took a single guitar track, stripped all the crazy solos and transposed all songs to the same key. Then I converted MIDIs to MusicXML to be able to parse them easily. [Input XMLs](/Runtime/XML/).

Basically, my method for music generation is the same as for generating text. So, we need to convert all our XMLs to a single text file. For that, I wrote a simple C# application ("MusicXML_to_TXT" folder). The text format is pretty straightforward. Each note is encoded like this: "NoteOctaveDuration", chord notes are connected with a plus sign, notes and chords are separated by spaces, measures - by "MeasureEnd", songs - by "SongEnd". [Text example](/Runtime/TXT/output.txt).

Note durations are encoded by special characters and stored in a [separate file](Runtime/TXT/durations.json), which we'll use later to decode generated text back to MusicXML.

For actual generation I'm using Tensorflow machine learning framework. The model is pretty simple and contains a single 256-unit LSTM layer. It is based on a Tensorflow's [text generation tutorial](https://www.tensorflow.org/tutorials/sequences/text_generation), but learning in this case is word-based. I tried a character-based too, but, as expected, it turned out less suitable and consistent. Python scripts for training the model and generating a new text could be found [here](/PythonTextGeneration/). At the end, the script executes the same C# application to convert generated text back to MusicXML, which later can be converted to MIDI or imported straight into the software sequencer.

### Project structure

* /MusicXML_to_TXT - Sources for the application to convert MusicXML to text and vice versa.
* /PythonTextGeneration - Python scripts for Tensorflow. All paths in the code should be changed to the actual ones.
  * text_gen_word_based.py - Trains a word-based model and generates a new text. This was the main script that I ended up using.
  * text_gen.py - Trains a character-based model and generates a new text.
  * text_gen_run.py - Generates a new text using a trained character-based model.
  * text_gen_run_word_based.py - Generates a new text using a trained word-based model.
* /Runtime - Runtime directory
  * /XML - Input data (one-track files in MusicXML format).
  * /TXT - Input data converted to text.
  * /GeneratedTXT - Generated text. Should also contain durations.json from the TXT directory.
  * /GeneratedXML - Generated text converted to MusicXML.  
  * MusicXML_to_TXT.exe - If run without arguments, it converts XMLs from the XML directory to text in the TXT directory. If run with any argument (e.g. MusicXML_to_TXT.exe 123), it converts text from the GeneratedTXT directory to XMLs in the GeneratedXML directory.
* /Template - Contains MusicXML template for the generated files.

### Prerequisites

Requires Python and [Tensorflow](https://www.tensorflow.org/) machine learning framework. 
At the moment, the model is configured to use CUDA, but that could be easily changed.
The application to convert MusicXML to text and vice versa runs on Windows.

### Useful links

* [Understanding LSTM](http://colah.github.io/posts/2015-08-Understanding-LSTMs/)
