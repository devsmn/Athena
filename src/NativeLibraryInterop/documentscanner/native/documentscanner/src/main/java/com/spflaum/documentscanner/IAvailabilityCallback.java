package com.spflaum.documentscanner;
import android.app.Activity;
import androidx.activity.ComponentActivity;
import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.IntentSenderRequest;
import androidx.activity.result.contract.ActivityResultContracts;


public interface IAvailabilityCallback {
	void onChecked(boolean available);
	void onError(Exception e);
}
