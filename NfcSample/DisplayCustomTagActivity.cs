using Android.Util;

namespace NfcXample
{
	using System;
	using System.Text;

	using Android.App;
	using Android.Nfc;
	using Android.OS;
	using Android.Widget;

	/// <summary>
	/// This activity will be used to display the image 
	/// for the string that was written to the NFC tag by MainActivity.
	/// </summary>
	[Activity, IntentFilter(new[] { "android.nfc.action.NDEF_DISCOVERED" },
		DataMimeType = MainActivity.DataMimeType,
		Categories = new[] { "android.intent.category.DEFAULT" })]
	public class DisplayHominidActivity : Activity
	{
		private ImageView nfcImage;
		//if you need to show an image "from nfc" this is the needed view
		private TextView nfcText;
		//if you need to show the text from nfc this is the view we're working with

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.DisplayCustomTagActivity);
			if (Intent == null)
			{
				return;
			}

			var intentType = Intent.Type ?? String.Empty;
			nfcImage = FindViewById<ImageView>(Resource.Id.iv_nfc);

			var button = FindViewById<Button>(Resource.Id.button_back_to_main_activity);
			button.Click += (sender, args) => Finish();

			nfcImage = FindViewById<ImageView>(Resource.Id.iv_nfc);
			nfcText = FindViewById<TextView>(Resource.Id.tv_nfcText);

			// MainActivity write the mimetype to the tag. We just do a quick check
			// to make sure that the tag that was discovered is indeed a tag that
			// this application wrote.
			if (MainActivity.DataMimeType.Equals(intentType))
			{
				// Get the string that was written to the NFC tag, and display it.
				var rawMessages = Intent.GetParcelableArrayExtra(NfcAdapter.ExtraNdefMessages);
				var msg = (NdefMessage)rawMessages[0];
				var ndefRecord = msg.GetRecords()[0];
				var ndefMessage = Encoding.ASCII.GetString(ndefRecord.GetPayload());

				DisplayTagAction(ndefMessage);//this method handles the business logic
				//DisplayHominid(ndefMessage); this is obsolete
			}
		}

		/// <summary>
		/// Displays the tag action.
		/// </summary>
		/// <param name="ndefMessage">Ndef message.</param>
		private void DisplayTagAction(string ndefMessage)
		{
			//TODO implement this method to show in the app what I need, by reading the nfc

			nfcText.Text = ndefMessage;
			Toast.MakeText(this, ndefMessage, ToastLength.Long).Show();//the toast is just for test
			Log.Info(MainActivity.Tag, ndefMessage);

			//switch (ndefMessage)
			//{
			//	case "1":
			//		break;
			//	case "2":
			//		break;
			//	default:
			//		break;
			//}


		}

		/// <summary>
		/// Display the image that is associated with the string in question.
		/// </summary>
		/// <param name="name"></param>
		[Obsolete("use DisplayTagAction(string ndefMessage) instead")]
		private void DisplayHominid(string name)
		{
			var hominidImageId = 0;

			if ("cornelius".Equals(name, StringComparison.OrdinalIgnoreCase))
			{
				hominidImageId = Resource.Drawable.cornelius;
			}
			if ("dr_zaius".Equals(name, StringComparison.OrdinalIgnoreCase))
			{
				hominidImageId = Resource.Drawable.dr_zaius;
			}
			if ("gorillas".Equals(name, StringComparison.OrdinalIgnoreCase))
			{
				hominidImageId = Resource.Drawable.gorillas;
			}
			if ("heston".Equals(name, StringComparison.OrdinalIgnoreCase))
			{
				hominidImageId = Resource.Drawable.heston;
			}

			if (hominidImageId > 0)
			{
				nfcImage.SetImageResource(hominidImageId);
			}
		}

	}//end of class (to be del)
}//end of namespace (to be del)
