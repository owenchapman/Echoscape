using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Security.AccessControl;
using System.Text;


public class Recorder : MonoBehaviour
{

		private FileStream fileStream;
		private MemoryStream audioStream;
		private string filePath;
		private int outputRate = 44100;
		private int headerSize = 44;
		//default for uncompressed wav
		public bool recOutput;
		private Blip recToggle;
		public string defaultDir;
		public string fileName;
		public RecordState recState;
		public float timeStamp;
		public bool loop;
		public bool autoName;
		public bool writeToFile;

		private int counter;
		private byte[] audiodata;
		private bool paused;
		
		private int trackCount = 8;
		// Use this for initialization
		void Start ()
		{

				outputRate = AudioSettings.outputSampleRate;
				counter = 0;
				timeStamp = 0f;
				paused = false;

				defaultDir = Path.Combine (Application.dataPath, "UserRecordings");
				//defaultDir = "/Users/dan/Documents/UserRecordings";


				if (Application.platform == RuntimePlatform.OSXEditor)
						defaultDir = "/Users/dan/Documents/UserRecordings";

				if (Application.platform == RuntimePlatform.WindowsEditor)
						defaultDir = @"C:\Users\Home\AppData\Roaming\Echoscape\";

				if (!Directory.Exists (defaultDir))
						Directory.CreateDirectory (defaultDir);

				fileName = "MyRecording.wav";

				recToggle = new Blip ();
				recOutput = false;

				autoName = true;

			

		}
		// Update is called once per frame
		void Update ()
		{


		}
	

		void OnAudioFilterRead (float[] data, int channels)
		{
				if (recOutput)
						ConvertAndWrite (data);
		}

		public void ToggleRecord (bool stream)
		{
				if (recToggle.Listen (stream)) {

						if (!recOutput) {
								Debug.Log ("initializing");
								timeStamp = Time.timeSinceLevelLoad;
								StartWriting ();
								recOutput = !recOutput;
						} else {
								recOutput = !recOutput;
								Debug.Log ("Cleaning Up");

								WriteHeader ();
								WriteToFile ();
									
			    				audioStream.Dispose ();    
          
						}
            
				}
		}

		void StartWriting ()
		{
				audioStream = new MemoryStream ();

				for (int i = 0; i < headerSize; i++)
						audioStream.WriteByte (0);
		}

		void ConvertAndWrite (float[] dataSource)
		{
				Int16[] intData = new Int16[dataSource.Length];
				Byte[] bytesData = new Byte[dataSource.Length * 2];
				Int16 rescaleFactor = 32767; //to convert float to Int16

				for (int i = 0; i < dataSource.Length; i++) {
						intData [i] = (short)(dataSource [i] * rescaleFactor);
						Byte[] byteArr = new Byte[2];
						byteArr = BitConverter.GetBytes (intData [i]);
						byteArr.CopyTo (bytesData, i * 2);
				}

				audioStream.Write (bytesData, 0, bytesData.Length);
		}

		void WriteToFile ()
		{
				if (!fileName.Contains (".wav"))
						fileName += ".wav";
      
				if (autoName)
						fileName = AutoName ();

				Debug.Log ("writing path");

				filePath = Path.Combine (defaultDir, fileName);

				if (File.Exists (filePath)) {
						Debug.Log ("File already exists, please rename...");
						return;
				}

				Debug.Log ("Writing file");
        
				fileStream = new FileStream (filePath, FileMode.Create);
				audioStream.WriteTo (fileStream);
				fileStream.Close ();

		}

		string AutoName ()
		{
				var tmpName = "Rec_";

				var t = DateTime.Now.TimeOfDay.ToString ();
				var nt = new char[8];

				for (int i = 0; i < 8; i++) {
						var c = (i + 1) % 3;

						if (c == 0)
								nt [i] = '_';
						else
								nt [i] = t [i];

				}

				tmpName += new string (nt);
				tmpName += ".wav";

				return tmpName;
		}

		void WriteHeader ()
		{
				audioStream.Seek (0, SeekOrigin.Begin);

				Byte[] riff = System.Text.Encoding.UTF8.GetBytes ("RIFF");
				audioStream.Write (riff, 0, 4);

				Byte[] chunkSize = BitConverter.GetBytes (audioStream.Length - 8);
				audioStream.Write (chunkSize, 0, 4);

				Byte[] wave = System.Text.Encoding.UTF8.GetBytes ("WAVE");
				audioStream.Write (wave, 0, 4);

				Byte[] fmt = System.Text.Encoding.UTF8.GetBytes ("fmt ");
				audioStream.Write (fmt, 0, 4);

				Byte[] subChunk1 = BitConverter.GetBytes (16);
				audioStream.Write (subChunk1, 0, 4);

				UInt16 two = 2;
				UInt16 one = 1;

				Byte[] audioFormat = BitConverter.GetBytes (one);
				audioStream.Write (audioFormat, 0, 2);

				Byte[] numChannels = BitConverter.GetBytes (two);
				audioStream.Write (numChannels, 0, 2);

				Byte[] sampleRate = BitConverter.GetBytes (outputRate);
				audioStream.Write (sampleRate, 0, 4);

				//sampleRate * bytesPerSample * number of channels, here 44100*2*2
				Byte[] byteRate = BitConverter.GetBytes (outputRate * 2 * 2);
				audioStream.Write (byteRate, 0, 4);

				UInt16 four = 4;
				Byte[] blockAlign = BitConverter.GetBytes (four);
				audioStream.Write (blockAlign, 0, 2);

				UInt16 sixteen = 16;
				Byte[] bitsPerSample = BitConverter.GetBytes (sixteen);
				audioStream.Write (bitsPerSample, 0, 2);

				Byte[] dataString = System.Text.Encoding.UTF8.GetBytes ("data");
				audioStream.Write (dataString, 0, 4);

				Byte[] subChunk2 = BitConverter.GetBytes (audioStream.Length - headerSize);
				audioStream.Write (subChunk2, 0, 4);

				audioStream.Seek (0, SeekOrigin.Begin);
		}
	
}
