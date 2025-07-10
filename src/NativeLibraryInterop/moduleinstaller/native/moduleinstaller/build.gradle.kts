plugins {
    id("com.android.library")
}

android {
    namespace = "com.spflaum.moduleinstaller"
    compileSdk = 34

    defaultConfig {
        minSdk = 21
    }

    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_1_8
        targetCompatibility = JavaVersion.VERSION_1_8
    }
}

dependencies {
    implementation("androidx.appcompat:appcompat:1.6.1")
    implementation("com.google.android.play:core:1.10.3") // for Play Core split install
    implementation("com.google.android.gms:play-services-base:18.1.0") // if needed
}