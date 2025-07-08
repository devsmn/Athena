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

public interface IScanCallback {
	void onScanned(String[] pageUris);
	void onError(Exception e);
}
