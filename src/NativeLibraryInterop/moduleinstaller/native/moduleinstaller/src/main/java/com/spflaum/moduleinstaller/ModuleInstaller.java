package com.spflaum.moduleinstaller;

import android.content.Context;
import android.util.Log;

import com.google.android.play.core.splitinstall.SplitInstallManager;
import com.google.android.play.core.splitinstall.SplitInstallManagerFactory;
import com.google.android.play.core.splitinstall.SplitInstallRequest;
import com.google.android.play.core.splitinstall.SplitInstallStateUpdatedListener;
import com.google.android.play.core.splitinstall.SplitInstallSessionState;

public class ModuleInstaller {

    public interface InstallCallback {
        void onSuccess();
        void onFailure(Exception e);
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

        SplitInstallStateUpdatedListener listener = state -> {
            if (state.moduleNames().contains(moduleName)) {
                switch (state.status()) {
                    case SplitInstallSessionStatus.INSTALLED:
                        callback.onSuccess();
                        manager.unregisterListener(this);
                        break;
                    case SplitInstallSessionStatus.FAILED:
                        callback.onFailure(new Exception("Module installation failed: " + state.errorCode()));
                        manager.unregisterListener(this);
                        break;
                }
            }
        };

        manager.registerListener(listener);
        manager.startInstall(request)
            .addOnFailureListener(callback::onFailure);
    }
}