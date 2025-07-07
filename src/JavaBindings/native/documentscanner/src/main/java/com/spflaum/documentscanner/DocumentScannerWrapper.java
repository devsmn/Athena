package com.spflaum.documentscanner;

import com.google.mlkit.vision.documentscanner.GmsDocumentScannerOptions;
import com.google.mlkit.vision.documentscanner.GmsDocumentScanning;
import android.content.IntentSender;
import android.app.Activity;


public class DocumentScannerWrapper {
    private final Activity activity;

    public DocumentScannerWrapper(Activity activity) {
        this.activity = activity;
    }

    public void launchScanner(int requestCode) {
        GmsDocumentScannerOptions options = new GmsDocumentScannerOptions.Builder()
			.setGalleryImportAllowed(false)
			.setPageLimit(1)
			.setResultFormats(
				GmsDocumentScannerOptions.RESULT_FORMAT_JPEG,
				GmsDocumentScannerOptions.RESULT_FORMAT_PDF
			)
			.setScannerMode(GmsDocumentScannerOptions.SCANNER_MODE_FULL)
			.build();

		GmsDocumentScanning.getClient(options)
			.getStartScanIntent(activity)
			.addOnSuccessListener(intentSender -> {
				try {
					activity.startIntentSenderForResult(intentSender, requestCode, null, 0, 0, 0);
				} catch (IntentSender.SendIntentException e) {
					e.printStackTrace(); // Or forward the error to your MAUI layer
				}
			})
			.addOnFailureListener(e -> {
				// Handle MLKit failure here
				e.printStackTrace();
			});
    }
}
