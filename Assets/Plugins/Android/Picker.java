package com.feiton.unimgpicker;

import android.app.Activity;
import android.app.Fragment;
import android.app.FragmentTransaction;
import android.content.Context;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.net.Uri;
import android.os.Bundle;

import com.unity3d.player.UnityPlayer;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.io.BufferedReader;
import java.io.InputStreamReader;

public class Picker extends Fragment
{
    private static final String TAG = "unimgpicker";
    private static final int REQUEST_CODE = 1;

    private static final String CALLBACK_OBJECT = "ImgPicker"; //"Unimgpicker"
    private static final String CALLBACK_METHOD = "OnComplete";
    private static final String CALLBACK_METHOD_FAILURE = "OnFailure";
    private static final String CALLBACK_METHOD_LOG = "OnLog";

    public static void show(String title, String outputFileName) {
        Activity unityActivity = UnityPlayer.currentActivity;
        if (unityActivity == null) {
            notifyFailure("Failed to open the picker");
            return;
        }

        Picker picker = new Picker();
        picker.mTitle = title;
        picker.mOutputFileName = outputFileName;

        FragmentTransaction transaction = unityActivity.getFragmentManager().beginTransaction();

        transaction.add(picker, TAG);
        transaction.commit();
    }

    private static void notifySuccess(String path) {
        UnityPlayer.UnitySendMessage(CALLBACK_OBJECT, CALLBACK_METHOD, path);
    }

    private static void notifyFailure(String cause) {
        UnityPlayer.UnitySendMessage(CALLBACK_OBJECT, CALLBACK_METHOD_FAILURE, cause);
    }
	
    private static void notifyLog(String log) {
        UnityPlayer.UnitySendMessage(CALLBACK_OBJECT, CALLBACK_METHOD_LOG, log);
    }	

    private String mTitle = "";
    private String mOutputFileName = "";
	
    public void onCreate(Bundle savedInstanceState ) {
        super.onCreate(savedInstanceState);

        Intent intent = new Intent(Intent.ACTION_OPEN_DOCUMENT);
        //intent.action = Intent.ACTION_OPEN_DOCUMENT;
        intent.setType("image/*");
        intent.addCategory(Intent.CATEGORY_OPENABLE);

        startActivityForResult(Intent.createChooser(intent, mTitle), REQUEST_CODE);
    }

	@Override
    public void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);

        if (requestCode != REQUEST_CODE) {
        	notifyFailure("Failed requestCode");
            return;
        }

        FragmentTransaction transaction = getActivity().getFragmentManager().beginTransaction();
        transaction.remove(this);
        transaction.commit();

        if (resultCode != Activity.RESULT_OK || data == null) {
            notifyFailure("Failed to pick the image");
            return;
        }

        Uri uri = data.getData();
    	//Uri uri = data.data;
        if (uri == null) {
            notifyFailure("Failed to pick the image");
            return;
        }

        Context context = getActivity().getApplicationContext();

        try {
            InputStream inputStream = context.getContentResolver().openInputStream(uri);
            if (inputStream == null) {
                notifyFailure("Failed to find the image");
                return;
            }

            OutputStream outputStream = context.openFileOutput(mOutputFileName, Context.MODE_PRIVATE);
            //val buffer = ByteArray(1024);
        	byte[] buffer = new byte[1024];
            int readLength = 0;
            while ((readLength = inputStream.read(buffer)) != -1) {
                outputStream.write(buffer, 0, readLength);
            }
            //while (inputStream.read(buffer) > 0) {
            //    outputStream.write(buffer, 0, readLength);
            //}
            //while ((inputStream.read(buffer).also { readLength = it; }) > 0) {
            //    outputStream.write(buffer, 0, readLength);
            //}
        } catch (FileNotFoundException e) {
            notifyFailure("Failed to find the image");
            return;
        } catch (IOException e) {
            notifyFailure("Failed to copy the image");
            return;
        }

        //notifySuccess(uri.getPath());

        File output = context.getFileStreamPath(mOutputFileName);
        notifySuccess(output.toString());
    	
        ////notifySuccess(output.path);
    }
}