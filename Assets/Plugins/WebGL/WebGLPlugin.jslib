mergeInto(LibraryManager.library, {
   // Function example
   CallFunction: function () {
      // Show a message as an alert
      window.showAdv();
   },
   // Function Share (add ksimaster)
   ShareFunction: function () {
      // Show a message as an alert
      window.showShare();
   },
   // Function GetLang(add ksimaster)
   GetLang: function () {
      var lang = sdk.environment.i18n.lang;
      var bufferSize = lengthBytesUTF8(lang) + 1;
      var buffer = _malloc(bufferSize);
      stringToUTF8(lang, buffer, bufferSize);
      return buffer;
   },
   // Function InterstitialFunction (add ksimaster)
   InterstitialFunction: function () {
      // Show a message as an alert
      window.showAdInterstitial();
   },
   // Function RewardFunction (add ksimaster)
   RewardFunction: function () {
      // Show a message as an alert
      window.showAdReward();
   },
      // Function SetEnglishFunction (add ksimaster)
   SetEnglishFunction: function () {
      SetEnglish();
   },
   // Function with the text param
   PassTextParam: function (text) {
      // Convert bytes to the text
      var convertedText = Pointer_stringify(text);
      // Show a message as an alert
      window.alert("You've passed the text: " + convertedText);
   },
   // Function with the number param
   PassNumberParam: function (number) {
      // Show a message as an alert
      window.alert("The number is: " + number);
   },
   // Function returning text value
   GetTextValue: function () {
      // Define text value
      var textToPass = "You got this text from the plugin";
      // Create a buffer to convert text to bytes
      var bufferSize = lengthBytesUTF8(textToPass) + 1;
      var buffer = _malloc(bufferSize);
      // Convert text
      stringToUTF8(textToPass, buffer, bufferSize);
      // Return text value
      return buffer;
   },
   // Function returning number value
   GetNumberValue: function () {
      // Return number value
      return 2020;
   }
});