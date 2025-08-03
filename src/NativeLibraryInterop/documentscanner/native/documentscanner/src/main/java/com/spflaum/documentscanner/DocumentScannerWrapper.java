package com.spflaum.documentscanner;

import android.app.Activity;
import androidx.activity.ComponentActivity;
import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.IntentSenderRequest;
import androidx.activity.result.contract.ActivityResultContracts;

import com.google.mlkit.vision.documentscanner.GmsDocumentScannerOptions;
import com.google.mlkit.vision.documentscanner.GmsDocumentScanning;
import com.google.mlkit.vision.documentscanner.GmsDocumentScanningResult;

import com.google.android.gms.common.api.OptionalModuleApi;
import com.google.android.gms.common.moduleinstall.ModuleInstall;
import com.google.android.gms.common.moduleinstall.ModuleInstallClient;
import com.google.android.gms.common.moduleinstall.ModuleAvailabilityResponse;

import java.util.ArrayList;
import java.util.List;

public class DocumentScannerWrapper {

    private ComponentActivity activity;
    private final IScanCallback callback;
    private ActivityResultLauncher<IntentSenderRequest> launcher;
    private GmsDocumentScannerOptions scanOptions;
	
    public DocumentScannerWrapper(IScanCallback callback) {
        this.callback = callback;
    }
	
	public void Initialize(Activity activity)
	{
		this.activity = (ComponentActivity)activity;
		
		this.launcher = this.activity.registerForActivityResult(
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

    public void isScannerInstalled(IAvailabilityCallback callback) {
        OptionalModuleApi scanner = GmsDocumentScanning.getClient(retrieveOptions());

        ModuleInstallClient mi = ModuleInstall.getClient(activity);
        mi.areModulesAvailable(scanner)
            .addOnSuccessListener((ModuleAvailabilityResponse resp) -> {
                callback.onChecked(resp.areModulesAvailable());
            })
            .addOnFailureListener(callback::onError);
        }

    public void launchScanner() {
        GmsDocumentScannerOptions options = retrieveOptions();

        GmsDocumentScanning.getClient(options)
            .getStartScanIntent(activity)
            .addOnSuccessListener(intentSender -> {
                launcher.launch(new IntentSenderRequest.Builder(intentSender).build());
            })
            .addOnFailureListener(callback::onError);
    }

    private GmsDocumentScannerOptions retrieveOptions(){
        if (scanOptions == null) {
            scanOptions = new GmsDocumentScannerOptions.Builder()
                .setGalleryImportAllowed(true) 
                .setPageLimit(1)
                .setResultFormats(GmsDocumentScannerOptions.RESULT_FORMAT_JPEG)
                .setScannerMode(GmsDocumentScannerOptions.SCANNER_MODE_FULL)
                .build();
        }

        return scanOptions;
    }
}
