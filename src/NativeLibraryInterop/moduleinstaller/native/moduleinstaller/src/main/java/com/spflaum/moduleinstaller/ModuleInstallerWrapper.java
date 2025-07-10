package com.spflaum.moduleinstaller;

import android.content.Context;
import android.util.Log;

import com.google.android.play.core.splitinstall.SplitInstallManager;
import com.google.android.play.core.splitinstall.SplitInstallManagerFactory;
import com.google.android.play.core.splitinstall.SplitInstallRequest;
import com.google.android.play.core.splitinstall.SplitInstallStateUpdatedListener;

public class ModuleInstallerWrapper {

    public interface InstallCallback {
        void onSuccess();
        void onFailure(Exception e);
		void onProgress(int percent);
    }

    public static void installModule(Context context, String moduleName, InstallCallback callback) {
        SplitInstallManager manager = SplitInstallManagerFactory.create(context);

        if (manager.getInstalledModules().contains(moduleName)) {
            callback.onSuccess();
            return;
        }

        SplitInstallRequest request = SplitInstallRequest.newBuilder()
            .addModule(moduleName)
            .build();

        SplitInstallStateUpdatedListener[] listener = new SplitInstallStateUpdatedListener[1];

		listener[0] = state -> {
			if (state.moduleNames().contains(moduleName)) {
				switch (state.status()) {
					 case 2: // SplitInstallSessionStatus.DOWNLOADING:
                        long totalBytes = state.totalBytesToDownload();
						long downloadedBytes = state.bytesDownloaded();
                        int percent = totalBytes > 0 ? (int)((downloadedBytes * 100L) / totalBytes) : 0;
                        callback.onProgress(percent); 
                        break;
						
					case 5: //SplitInstallSessionStatus.INSTALLED:
						callback.onSuccess();
						manager.unregisterListener(listener[0]); 
						break;
						
					case 6: //SplitInstallSessionStatus.FAILED:
						callback.onFailure(new Exception("Module installation failed: " + state.errorCode()));
						manager.unregisterListener(listener[0]);
						break;
				}
			}
		};

		manager.registerListener(listener[0]);
        manager.startInstall(request)
            .addOnFailureListener(callback::onFailure);
    }
}