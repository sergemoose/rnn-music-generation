import tensorflow as tf
import numpy
tf.enable_eager_execution()
import numpy as np
import io

#path_to_file = tf.keras.utils.get_file('shakespeare.txt', 'https://storage.googleapis.com/download.tensorflow.org/data/shakespeare.txt')
path_to_file = 'g:\\net\\GuitarPro_to_TXT\\TXT\\output.txt'
f = io.open(path_to_file, mode="r", encoding="utf-8")
text = f.read()

words = text.split(' ')
# delete last line-break
del words[-1]

vocab = sorted(set(words))

# Creating a mapping from unique characters to indices
char2idx = {u:i for i, u in enumerate(vocab)}
idx2char = np.array(vocab)

# Length of the vocabulary in chars
vocab_size = len(vocab)

# The embedding dimension 
embedding_dim = 256

# Number of RNN units
#rnn_units = 64
rnn_units = 1024


def build_model(vocab_size, embedding_dim, rnn_units, batch_size):
  model = tf.keras.Sequential([
    tf.keras.layers.Embedding(vocab_size, embedding_dim, 
                              batch_input_shape=[batch_size, None]),
    tf.keras.layers.CuDNNGRU(rnn_units,
        return_sequences=True, 
        recurrent_initializer='glorot_uniform',
        stateful=True),
    tf.keras.layers.Dense(vocab_size)
  ])
  return model

model = build_model(
  vocab_size = len(vocab), 
  embedding_dim=embedding_dim, 
  rnn_units=rnn_units, 
  batch_size=1)


checkpoint_dir = 'g:\\Serge\\Save\\Python\\RNN'

model = build_model(vocab_size, embedding_dim, rnn_units, batch_size=1)

model.load_weights(tf.train.latest_checkpoint(checkpoint_dir))

model.build(tf.TensorShape([1, None]))



def generate_text(model, start_string):
  # Evaluation step (generating text using the learned model)

  # Number of characters to generate
  num_generate = 500

  # Converting our start string to numbers (vectorizing) 
  input_eval = [char2idx[s] for s in start_string.split(' ')]
  input_eval = tf.expand_dims(input_eval, 0)

  # Empty string to store our results
  text_generated = []

  # Low temperatures results in more predictable text.
  # Higher temperatures results in more surprising text.
  # Experiment to find the best setting.
  temperature = 1

  # Here batch size == 1
  model.reset_states()
  for i in range(num_generate):
      predictions = model(input_eval)
      # remove the batch dimension
      predictions = tf.squeeze(predictions, 0)

      # using a multinomial distribution to predict the word returned by the model
      predictions = predictions / temperature
      predicted_id = tf.multinomial(predictions, num_samples=1)[-1,0].numpy()
      
      # We pass the predicted word as the next input to the model
      # along with the previous hidden state
      input_eval = tf.expand_dims([predicted_id], 0)
      
      text_generated.append(idx2char[predicted_id])

  return (start_string + ' ' + ' '.join(text_generated))



generated_text = generate_text(model, start_string="E3û E3û")

with open("g:\\net\\GuitarPro_to_TXT\\GeneratedTXT\\input.txt", mode="w", encoding="utf-8") as text_file:
    text_file.write(generated_text)


os.chdir('g:\\net\\GuitarPro_to_TXT')
os.system('"g:\\net\\GuitarPro_to_TXT\\MusicXML_to_TXT.exe 123"')