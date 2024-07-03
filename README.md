# Huffman Coding

Huffman Coding written in .NET 8.0 C#

## Overview of Huffman Coding:
Under the ASCII encoding standard a single character takes up 1 byte or 8 bits, Huffman coding instead attempts to minimize the number of bits used to encode the most common characters at the expense of the least used characters. Meaning commonly used characters will use fewer than 8 bits but an uncommon chatacter might use more than 8 bits. The goal is that on average it uses fewer bits since it uses fewer bits on the most used characters.

## How to Use:
All the functionality needed to serialize and deserialize text is inside the [HuffmanSerializer](HuffmanSerializer.cs) class which converts the input text and Huffman tree needed to decode it to binary data and back again.

To serialize the data simply call the .SerializeText function to get the binary data as a byte[]:

````
byte[] filePayload = HuffmanSerializer.SerializeText(originalText);
````

And to deserialize it back to a string you put the serialized data into the .DeserializeText function:

````
string originalText = HuffmanSerializer.DeserializeText(filePayload);
````
