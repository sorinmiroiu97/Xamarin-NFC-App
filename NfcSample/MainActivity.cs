using Java.Lang;
namespace NfcXample
{
	using System;
	using System.Text;

	using Android.App;
	using Android.Content;
	using Android.Nfc;
	using Android.Nfc.Tech;
	using Android.OS;
	using Android.Util;
	using Android.Views;
	using Android.Widget;

	using Java.IO;

	[Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{
		/// <summary>
		/// A mime type for the string that this app will write to the NFC tag. Will be
		/// used to help this application identify NFC tags that is has written to.
		/// </summary>
		public const string DataMimeType = "application/vnd.xamarin.nfcxample";

		public static readonly string NfcAppRecord = "xamarin.nfxample";
		public static readonly string Tag = "NfcXample";

		private bool inWriteMode;
		private NfcAdapter nfcAdapter;
		private TextView textInfo; //will need refactoring
		private Button writeTagButton;
		private EditText input;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.MainActivity);

			// Get a reference to the default NFC adapter for this device. This adapter 
			// is how an Android application will interact with the actual hardware.
			nfcAdapter = NfcAdapter.GetDefaultAdapter(this);

			writeTagButton = FindViewById<Button>(Resource.Id.button_write_tag);
			writeTagButton.Click += OnClickWriteTagButton;

			textInfo = FindViewById<TextView>(Resource.Id.tv_info);
			input = FindViewById<EditText>(Resource.Id.et_tag_input);
		}

		/// <summary>
		/// This method is called when an NFC tag is discovered by the application.
		/// </summary>
		/// <param name="intent"></param>
		protected override void OnNewIntent(Intent intent)
		{
			if (inWriteMode)
			{
				inWriteMode = false;
				var tag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Tag;

				if (tag == null)
				{
					return;
				}

				// These next few lines will create a payload (consisting of a string)
				// and a mimetype. NFC record are arrays of bytes. 

				//var payload = Encoding.ASCII.GetBytes(GetRandomHominid());
				//replace getrandomhominid with -> GetEditTextInput()

				var payload = Encoding.ASCII.GetBytes(GetEditTextInput());
				var mimeBytes = Encoding.ASCII.GetBytes(DataMimeType);
				var ndefRecord = new NdefRecord(NdefRecord.TnfMimeMedia, mimeBytes, new byte[0], payload);
				var ndefMessage = new NdefMessage(new[] { ndefRecord });
				//ndefMessage is the var that holds the message that goes into the nfc chip

				if (!TryAndWriteToTag(tag, ndefMessage))
				{
					// Maybe the write couldn't happen because the tag wasn't formatted?
					TryAndFormatTagWithMessage(tag, ndefMessage);
				}
			}
		}

		protected override void OnPause()
		{
			base.OnPause();
			// App is paused, so no need to keep an eye out for NFC tags.
			if (nfcAdapter != null)
				nfcAdapter.DisableForegroundDispatch(this);
		}

		/// <summary>
		/// Displays the message into a text view and into a new toast.
		/// </summary>
		/// <param name="ndefMessage">Message.</param>
		private void DisplayMessage(string ndefMessage)
		{
			textInfo.Text = ndefMessage;
			Toast.MakeText(this, ndefMessage, ToastLength.Long).Show();
			//the toast is just for test
			Log.Info(Tag, ndefMessage);
		}

		/// <summary>
		/// Identify to Android that this activity wants to be notified when 
		/// an NFC tag is discovered. 
		/// </summary>
		private void EnableWriteMode()
		{
			inWriteMode = true;

			// Create an intent filter for when an NFC tag is discovered.  When
			// the NFC tag is discovered, Android will u
			var tagDetected = new IntentFilter(NfcAdapter.ActionTagDiscovered);
			var filters = new[] { tagDetected };

			// When an NFC tag is detected, Android will use the PendingIntent to come back to this activity.
			// The OnNewIntent method will invoked by Android.
			var intent = new Intent(this, GetType()).AddFlags(ActivityFlags.SingleTop);
			var pendingIntent = PendingIntent.GetActivity(this, 0, intent, 0);

			if (nfcAdapter == null)
			{
				var alert = new AlertDialog.Builder(this).Create();
				alert.SetMessage("NFC is not supported on this device.");
				alert.SetTitle("NFC Unavailable");
				alert.SetButton("OK", delegate
				{
					writeTagButton.Enabled = false;
					textInfo.Text = "NFC is not supported on this device.";
				});
				alert.Show();
			}
			else
				nfcAdapter.EnableForegroundDispatch(this, pendingIntent, filters, null);
		}

		/// <summary>
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="ndefMessage"></param>
		/// <returns>a boolean, true if it succeeded, else false</returns>
		private bool TryAndFormatTagWithMessage(Tag tag, NdefMessage ndefMessage)
		{
			var format = NdefFormatable.Get(tag);
			if (format == null)
			{
				DisplayMessage("Tag does not appear to support NDEF format.");
			}
			else
			{
				try
				{
					format.Connect();
					format.Format(ndefMessage);
					DisplayMessage("Tag successfully written.");
					return true;
				}
				catch (IOException ioex)
				{
					var msg = "There was an error trying to format the tag.";
					DisplayMessage(msg);
					Log.Error(Tag, ioex, msg);
				}
			}
			return false;
		}

		/// <summary>
		/// Pick one of the four hominids to display. This should be replaced with the business logic I choose for the app.
		/// </summary>
		/// <returns>A string that corresponds to one of the images in this application.</returns>
		[Obsolete("You should use the GetEditTextInput() method instead", true)]
		private string GetRandomHominid() //this was replaced with the method down below
		{
			var random = new Random();
			var r = random.NextDouble();
			Log.Debug(Tag, "Random number: {0}", r.ToString("N2"));
			if (r < 0.25)
			{
				return "heston";
			}
			if (r < 0.5)
			{
				return "gorillas";
			}
			if (r < 0.75)
			{
				return "dr_zaius";
			}
			return "cornelius";
		}

		///<summary>
		///Converts the input into a string from the edit text view 
		///</summary>
		/// <returns>The string input.</returns>
		private string GetEditTextInput() 
		{
			var text = this.input.Text.ToString();
			//var text = input.ToString();

			if(text.Equals("") || text == null)
			{
				DisplayMessage("No input detected, please type a string into the text box");
				throw new StringIndexOutOfBoundsException("no input detected");
				//this is just to prevent writing an empty string into the nfc chip
			}

			return text;
		}

		private void OnClickWriteTagButton(object sender, EventArgs eventArgs)
		{
			var view = (View)sender;
			if (view.Id == Resource.Id.button_write_tag)
			{
				DisplayMessage("Touch and hold the tag against the phone to write.");
				EnableWriteMode();
			}
		}

		/// <summary>
		/// This method will try and write the specified message to the provided tag. 
		/// </summary>
		/// <param name="tag">The NFC tag that was detected.</param>
		/// <param name="ndefMessage">An NDEF message to write.</param>
		/// <returns>true if the tag was written to.</returns>
		private bool TryAndWriteToTag(Tag tag, NdefMessage ndefMessage)
		{

			// This object is used to get information about the NFC tag as 
			// well as perform operations on it.
			var ndef = Ndef.Get(tag);
			if (ndef != null)
			{
				ndef.Connect();

				// Once written to, a tag can be marked as read-only - check for this.
				if (!ndef.IsWritable)
				{
					DisplayMessage("Tag is read-only.");
				}

				// NFC tags can only store a small amount of data, this depends on the type of tag it is.
				var size = ndefMessage.ToByteArray().Length;
				if (ndef.MaxSize < size)
				{
					DisplayMessage("Tag doesn't have enough space.");
				}

				ndef.WriteNdefMessage(ndefMessage);
				DisplayMessage("Succesfully wrote tag.");
				return true;
			}

			return false;
		}
	}
}
