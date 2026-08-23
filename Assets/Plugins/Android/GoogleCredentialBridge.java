package com.hong.copspace.auth;

import android.app.Activity;

import androidx.annotation.NonNull;
import androidx.credentials.Credential;
import androidx.credentials.CredentialManager;
import androidx.credentials.CredentialManagerCallback;
import androidx.credentials.CustomCredential;
import androidx.credentials.GetCredentialRequest;
import androidx.credentials.GetCredentialResponse;
import androidx.credentials.exceptions.GetCredentialCancellationException;
import androidx.credentials.exceptions.GetCredentialException;
import androidx.credentials.exceptions.NoCredentialException;

import com.google.android.libraries.identity.googleid.GetGoogleIdOption;
import com.google.android.libraries.identity.googleid.GoogleIdTokenCredential;
import com.unity3d.player.UnityPlayer;

import java.util.concurrent.Executor;

public final class GoogleCredentialBridge
{
    private GoogleCredentialBridge()
    {
    }

    public static void signIn(
        Activity activity,
        String serverClientId,
        String receiverGameObject,
        String successMethod,
        String errorMethod)
    {
        if (activity == null)
        {
            sendError(
                receiverGameObject,
                errorMethod,
                "ERROR|Android Activity is null."
            );

            return;
        }

        if (serverClientId == null ||
            serverClientId.trim().isEmpty())
        {
            sendError(
                receiverGameObject,
                errorMethod,
                "ERROR|Web Client ID is empty."
            );

            return;
        }

        requestCredential(
            activity,
            serverClientId,
            receiverGameObject,
            successMethod,
            errorMethod,
            true
        );
    }

    private static void requestCredential(
        Activity activity,
        String serverClientId,
        String receiverGameObject,
        String successMethod,
        String errorMethod,
        boolean authorizedOnly)
    {
        activity.runOnUiThread(
            new Runnable()
            {
                @Override
                public void run()
                {
                    try
                    {
                        GetGoogleIdOption googleIdOption =
                            new GetGoogleIdOption.Builder()
                                .setFilterByAuthorizedAccounts(
                                    authorizedOnly
                                )
                                .setServerClientId(
                                    serverClientId
                                )
                                .setAutoSelectEnabled(
                                    false
                                )
                                .build();

                        GetCredentialRequest request =
                            new GetCredentialRequest.Builder()
                                .addCredentialOption(
                                    googleIdOption
                                )
                                .build();

                        CredentialManager
                            credentialManager =
                                CredentialManager.create(
                                    activity
                                );

                        Executor mainExecutor =
                            new Executor()
                            {
                                @Override
                                public void execute(
                                    Runnable command)
                                {
                                    activity.runOnUiThread(
                                        command
                                    );
                                }
                            };

                        credentialManager
                            .getCredentialAsync(
                                activity,
                                request,
                                null,
                                mainExecutor,
                                new CredentialManagerCallback
                                    <
                                        GetCredentialResponse,
                                        GetCredentialException
                                    >()
                                {
                                    @Override
                                    public void onResult(
                                        GetCredentialResponse
                                            response)
                                    {
                                        handleCredential(
                                            response,
                                            receiverGameObject,
                                            successMethod,
                                            errorMethod
                                        );
                                    }

                                    @Override
                                    public void onError(
                                        @NonNull
                                        GetCredentialException
                                            exception)
                                    {
                                        // 사용자가 닫거나 뒤로가기.
                                        if (exception
                                            instanceof
                                            GetCredentialCancellationException)
                                        {
                                            sendError(
                                                receiverGameObject,
                                                errorMethod,
                                                "CANCELLED|" +
                                                safeMessage(
                                                    exception
                                                )
                                            );

                                            return;
                                        }

                                        // 기존 승인 계정이 없으면
                                        // 기기의 전체 Google 계정을
                                        // 대상으로 다시 요청한다.
                                        if (authorizedOnly &&
                                            exception
                                                instanceof
                                                NoCredentialException)
                                        {
                                            requestCredential(
                                                activity,
                                                serverClientId,
                                                receiverGameObject,
                                                successMethod,
                                                errorMethod,
                                                false
                                            );

                                            return;
                                        }

                                        sendError(
                                            receiverGameObject,
                                            errorMethod,
                                            "ERROR|" +
                                            exception
                                                .getClass()
                                                .getSimpleName()
                                            +
                                            "|"
                                            +
                                            safeMessage(
                                                exception
                                            )
                                        );
                                    }
                                }
                            );
                    }
                    catch (Exception exception)
                    {
                        sendError(
                            receiverGameObject,
                            errorMethod,
                            "ERROR|" +
                            exception
                                .getClass()
                                .getSimpleName()
                            +
                            "|"
                            +
                            safeMessage(
                                exception
                            )
                        );
                    }
                }
            }
        );
    }

    private static void handleCredential(
        GetCredentialResponse response,
        String receiverGameObject,
        String successMethod,
        String errorMethod)
    {
        try
        {
            if (response == null)
            {
                sendError(
                    receiverGameObject,
                    errorMethod,
                    "ERROR|Credential response is null."
                );

                return;
            }

            Credential credential =
                response.getCredential();

            if (!(credential
                instanceof CustomCredential))
            {
                sendError(
                    receiverGameObject,
                    errorMethod,
                    "ERROR|Unexpected credential class."
                );

                return;
            }

            CustomCredential customCredential =
                (CustomCredential)credential;

            String credentialType =
                customCredential.getType();

            if (!GoogleIdTokenCredential
                .TYPE_GOOGLE_ID_TOKEN_CREDENTIAL
                .equals(
                    credentialType
                ))
            {
                sendError(
                    receiverGameObject,
                    errorMethod,
                    "ERROR|Unexpected credential type: "
                    +
                    credentialType
                );

                return;
            }

            GoogleIdTokenCredential
                googleCredential =
                    GoogleIdTokenCredential
                        .createFrom(
                            customCredential
                                .getData()
                        );

            String idToken =
                googleCredential
                    .getIdToken();

            if (idToken == null ||
                idToken.isEmpty())
            {
                sendError(
                    receiverGameObject,
                    errorMethod,
                    "ERROR|Google ID Token is empty."
                );

                return;
            }

            UnityPlayer.UnitySendMessage(
                receiverGameObject,
                successMethod,
                idToken
            );
        }
        catch (Exception exception)
        {
            sendError(
                receiverGameObject,
                errorMethod,
                "ERROR|" +
                exception
                    .getClass()
                    .getSimpleName()
                +
                "|"
                +
                safeMessage(
                    exception
                )
            );
        }
    }

    private static void sendError(
        String receiverGameObject,
        String errorMethod,
        String message)
    {
        UnityPlayer.UnitySendMessage(
            receiverGameObject,
            errorMethod,
            message == null
                ? "ERROR|Unknown"
                : message
        );
    }

    private static String safeMessage(
        Exception exception)
    {
        if (exception == null)
        {
            return "Unknown";
        }

        String message =
            exception.getMessage();

        return message == null
            ? "Unknown"
            : message;
    }
}