import tensorflow as tf
import numpy
tf.enable_eager_execution()
import matplotlib.pyplot as plt
import numpy as np
import os
import time
import io

path_to_file = 'g:\\net\\GuitarPro_to_TXT\\TXT\\output.txt'
f = io.open(path_to_file, mode="r", encoding="utf-8")
text = f.read()

words = text.split(' ')
# delete last line-break
del words[-1]

# length of text is the number of characters in it
print ('Length of text: {} words'.format(len(words)))

# The unique characters in the file
vocab = sorted(set(words))
print ('{} unique words'.format(len(vocab)))

# Creating a mapping from unique characters to indices
char2idx = {u:i for i, u in enumerate(vocab)}
idx2char = np.array(vocab)

words_as_int = np.array([char2idx[w] for w in words])

# The maximum length sentence we want for a single input in characters
seq_length = 100
examples_per_epoch = len(words)//seq_length

# Create training examples / targets
word_dataset = tf.data.Dataset.from_tensor_slices(words_as_int)
sequences = word_dataset.batch(seq_length+1, drop_remainder=True)

def split_input_target(chunk):
    input_text = chunk[:-1]
    target_text = chunk[1:]
    return input_text, target_text

dataset = sequences.map(split_input_target)

# Batch size 
BATCH_SIZE = 64
steps_per_epoch = examples_per_epoch//BATCH_SIZE

# Buffer size to shuffle the dataset
# (TF data is designed to work with possibly infinite sequences, 
# so it doesn't attempt to shuffle the entire sequence in memory. Instead, 
# it maintains a buffer in which it shuffles elements).
BUFFER_SIZE = 10000

dataset = dataset.shuffle(BUFFER_SIZE).batch(BATCH_SIZE, drop_remainder=True)

# Length of the vocabulary in chars
vocab_size = len(vocab)

# The embedding dimension 
embedding_dim = 256

# Number of RNN units
rnn_units = 256
#rnn_units = 64
#rnn_units = 1024


def build_model(vocab_size, embedding_dim, rnn_units, batch_size):
  model = tf.keras.Sequential([
    tf.keras.layers.Embedding(vocab_size, embedding_dim, 
                              batch_input_shape=[batch_size, None]),
    tf.keras.layers.CuDNNLSTM(rnn_units,
        return_sequences=True, 
        recurrent_initializer='glorot_uniform',
        recurrent_regularizer=tf.keras.regularizers.l2(0.01),
        stateful=True),                                                            
#    tf.keras.layers.CuDNNGRU(rnn_units,
#        return_sequences=True, 
#        recurrent_initializer='glorot_uniform',
#        recurrent_regularizer=tf.keras.regularizers.l2(0.01),
#        stateful=True
#        ),
    tf.keras.layers.Dense(vocab_size)
  ])
  return model

model = build_model(
  vocab_size = len(vocab), 
  embedding_dim=embedding_dim, 
  rnn_units=rnn_units, 
  batch_size=BATCH_SIZE)

model.compile(
    optimizer = tf.train.AdamOptimizer(),
    loss = tf.losses.sparse_softmax_cross_entropy)

# Directory where the checkpoints will be saved
checkpoint_dir = 'g:\\Serge\\Save\\Python\\RNN'
# Name of the checkpoint files
checkpoint_prefix = os.path.join(checkpoint_dir, "ckpt_{epoch}")

# removeing existing checkpoints
for the_file in os.listdir(checkpoint_dir):
    file_path = os.path.join(checkpoint_dir, the_file)
    try:
        if os.path.isfile(file_path):
            os.unlink(file_path)
    except Exception as e:
        print(e)


checkpoint_callback=tf.keras.callbacks.ModelCheckpoint(
    filepath=checkpoint_prefix,
    save_weights_only=True)

#EPOCHS=20
EPOCHS=100

history = model.fit(dataset.repeat(), epochs=EPOCHS, steps_per_epoch=steps_per_epoch, callbacks=[checkpoint_callback])

# summarize history for loss
plt.plot(history.history['loss'])
plt.title('model loss')
plt.ylabel('loss')
plt.xlabel('epoch')
plt.legend(['train'], loc='upper left')
plt.show()


def generate_text(model, start_string):
  # Evaluation step (generating text using the learned model)

  # Number of characters to generate
  num_generate = 6000

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




model = build_model(vocab_size, embedding_dim, rnn_units, batch_size=1)
model.load_weights(tf.train.latest_checkpoint(checkpoint_dir))
model.build(tf.TensorShape([1, None]))

generated_text = generate_text(model, start_string="E3û E3û")

with open("g:\\net\\GuitarPro_to_TXT\\GeneratedTXT\\input.txt", mode="w", encoding="utf-8") as text_file:
    text_file.write(generated_text)


os.chdir('g:\\net\\GuitarPro_to_TXT')
os.system('"g:\\net\\GuitarPro_to_TXT\\MusicXML_to_TXT.exe 123"')