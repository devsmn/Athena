package com.spflaum.documentscanner;

import android.app.Activity;
import androidx.activity.ComponentActivity;
import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.IntentSenderRequest;
import androidx.activity.result.contract.ActivityResultContracts;

import com.google.mlkit.vision.documentscanner.GmsDocumentScannerOptions;
import com.google.mlkit.vision.documentscanner.GmsDocumentScanning;
import com.google.mlkit.vision.documentscanner.GmsDocumentScanningResult;

import java.util.ArrayList;
import java.util.List;

public class DocumentScannerWrapper {


    private final ComponentActivity activity;
    private final IScanCallback callback;
    private final ActivityResultLauncher<IntentSenderRequest> launcher;
	
	public DocumentScannerWrapper(Activity activity, IScanCallback callback) {
		this((ComponentActivity) activity, callback);
	}

    public DocumentScannerWrapper(ComponentActivity activity, IScanCallback callback) {
        this.activity = activity;
        this.callback = callback;

        this.launcher = activity.registerForActivityResult(
            new ActivityResultContracts.StartIntentSenderForResult(),
            result -> {
                if (result.getResultCode() == Activity.RESULT_OK && result.getData() != null) {
                    try {
                        GmsDocumentScanningResult res =
                            GmsDocumentScanningResult.fromActivityResultIntent(result.getData());

                        if (res == null || res.getPages() == null) {
                            throw new RuntimeException("Scan result is null or contains no pages.");
                        }

                        List<String> pages = new ArrayList<>();
                        for (GmsDocumentScanningResult.Page pg : res.getPages()) {
                            pages.add(pg.getImageUri().toString());
                        }

                        callback.onScanned(pages.toArray(new String[0]));

                    } catch (Exception ex) {
                        callback.onError(ex);
                    }
                } else {
                    callback.onError(new RuntimeException("Scan canceled or failed."));
                }
            }
        );
    }

    public void launchScanner() {
        GmsDocumentScannerOptions options = new GmsDocumentScannerOptions.Builder()
            .setGalleryImportAllowed(true) // change to false if you want camera-only
            .setPageLimit(1)
            .setResultFormats(GmsDocumentScannerOptions.RESULT_FORMAT_JPEG)
            .setScannerMode(GmsDocumentScannerOptions.SCANNER_MODE_FULL)
            .build();

        GmsDocumentScanning.getClient(options)
            .getStartScanIntent(activity)
            .addOnSuccessListener(intentSender -> {
                launcher.launch(new IntentSenderRequest.Builder(intentSender).build());
            })
            .addOnFailureListener(callback::onError);
    }
}
