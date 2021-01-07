/*
 * Copyright (c) 2016, salesforce.com, inc. All rights reserved. Licensed under the BSD 3-Clause license. For full
 * license text, see LICENSE.TXT file in the repo root or https://opensource.org/licenses/BSD-3-Clause
 */
package com.salesforce.emp.connector.example;

import java.io.ByteArrayInputStream;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.lang.reflect.Array;
import java.net.*;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.nio.charset.StandardCharsets;
import java.security.*;
import java.security.cert.CertificateException;
import java.text.MessageFormat;
import java.util.*;
import java.util.concurrent.TimeUnit;
import java.util.function.Consumer;

import com.azure.identity.ClientSecretCredential;
import com.azure.identity.ClientSecretCredentialBuilder;
import com.azure.security.keyvault.secrets.SecretClient;
import com.azure.security.keyvault.secrets.SecretClientBuilder;
import com.azure.security.keyvault.secrets.models.KeyVaultSecret;
import com.fasterxml.jackson.core.type.TypeReference;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.microsoft.azure.eventgrid.EventGridClient;
import com.microsoft.azure.eventgrid.TopicCredentials;
import com.microsoft.azure.eventgrid.implementation.EventGridClientImpl;
import com.microsoft.azure.eventgrid.models.EventGridEvent;
import com.salesforce.emp.connector.BayeuxParameters;
import com.salesforce.emp.connector.EmpConnector;
import com.salesforce.emp.connector.TopicSubscription;
import org.cometd.bayeux.Channel;
import org.eclipse.jetty.util.ajax.JSON;
import org.joda.time.DateTime;

/**
 * An example of using the EMP connector using bearer tokens
 *
 * @author hal.hildebrand
 * @since API v37.0
 */
public class BearerTokenExample {
    private static final TopicCredentials topicCredentials = new TopicCredentials("tGOxjU3/ZvKJirlZ4cTxUH0c+ciMRFVD4Ye74/TjeOs=");
    private static final EventGridClient eventGridClient = new EventGridClientImpl(topicCredentials);
    private static final String TopicEndpoint = "https://contact-insert-update.eastus2-1.eventgrid.azure.net";

    private static final ClientSecretCredential credential = new ClientSecretCredentialBuilder()
            .tenantId("086c5b2d-4ac7-4e86-8f49-ac41b36aa6f6")
            .clientId("bc60f2f9-d6e0-4a06-8114-53c3c8bc1104")
            .clientSecret("pqoq8-Qrjil_rB0_yUnK6_ZHvjE8Q148CE")
            .build();

    private static final SecretClient secretClient = new SecretClientBuilder()
            .vaultUrl("https://astsalesforcekeyvault.vault.azure.net/")
            .credential(credential)
            .buildClient();

    public static void main(String[] argv) throws Exception {


        long replayFrom = EmpConnector.REPLAY_FROM_TIP;
        String token = getBearerToken(createJWT());

        BayeuxParameters params = new BayeuxParameters() {

            @Override
            public String bearerToken() {
                return token;
            }

            @Override
            public URL host() {
                try {
                    return new URL("https://501software-dev-ed.my.salesforce.com");
                } catch (MalformedURLException e) {
                    throw new IllegalArgumentException(String.format("Unable to create url: %s", argv[0]), e);
                }
            }
        };

        Consumer<Map<String, Object>> consumer = event -> {
            System.out.println(String.format("Received:\n%s", JSON.toString(event)));
            try {
                pushToAzure(event);
            } catch (Exception e) {
                e.printStackTrace();
            }
        };

        EmpConnector connector = new EmpConnector(params);

        connector.addListener(Channel.META_CONNECT, new LoggingListener(true, true))
        .addListener(Channel.META_DISCONNECT, new LoggingListener(true, true))
        .addListener(Channel.META_HANDSHAKE, new LoggingListener(true, true));

        connector.start().get(5, TimeUnit.SECONDS);

        TopicSubscription subscription = connector.subscribe("/data/ChangeEvents", replayFrom, consumer).get(5, TimeUnit.SECONDS);
        System.out.println(String.format("Subscribed: %s", subscription));
    }

    private static String getBearerToken(String jwt) throws URISyntaxException, IOException, InterruptedException {

        StringBuilder content = new StringBuilder();
        content.append( "grant_type=urn:ietf:params:oauth:grant-type:jwt-bearer&" );
        content.append("assertion=" + jwt);

        HttpRequest request = HttpRequest.newBuilder()
                .uri(new URI("https://501software-dev-ed.my.salesforce.com/services/oauth2/token"))
                .header("Content-Type", "application/x-www-form-urlencoded")
                .POST(HttpRequest.BodyPublishers.ofString(content.toString()))
                .build();

        HttpResponse<String> response = HttpClient.newHttpClient().send(request, HttpResponse.BodyHandlers.ofString());

        ObjectMapper mapper = new ObjectMapper();
        TypeReference<HashMap<String, String>> typeRef = new TypeReference<HashMap<String, String>>() {};
        Map<String, String> map = mapper.readValue(response.body(), typeRef);

        return map.get("access_token");
    }
    private static String createJWT() throws NoSuchProviderException, KeyStoreException, IOException, CertificateException, NoSuchAlgorithmException, UnrecoverableKeyException, InvalidKeyException, SignatureException {
        final String header = "{\"alg\":\"RS256\"}";
        final String claimTemplate = "'{'\"iss\": \"{0}\", \"sub\": \"{1}\", \"aud\": \"{2}\", \"exp\": \"{3}\"'}'";

        StringBuilder token = new StringBuilder();
        token.append(Base64.getUrlEncoder().encodeToString(header.getBytes(StandardCharsets.UTF_8)));
        token.append(".");

        String[] claims = new String[4];
        claims[0]= "3MVG9l2zHsylwlpTGlLcDC3b.xHPxfs.Q6QyHhdWKE8_xgBvPze4ETY7VKGCOj30od6oXv1nRDQBzO3KiXmb8";
        claims[1]= "b.simon@culhamdagmail.onmicrosoft.com";
        claims[2]="https://login.salesforce.com";
        claims[3]= Long.toString((System.currentTimeMillis() / 1000 ) + 300 );

        MessageFormat claimFormat = new MessageFormat(claimTemplate);
        String payload = claimFormat.format(claims);
        token.append(Base64.getUrlEncoder().encodeToString(payload.getBytes(StandardCharsets.UTF_8)));

        KeyStore ks = KeyStore.getInstance("pkcs12", "SunJSSE");
        KeyVaultSecret secret = secretClient.getSecret("Salesforce");
        byte[] certBytes = Base64.getDecoder().decode(secret.getValue());

        //ks.load(new FileInputStream("C:/Users/athie/salesforce.pfx"), new char[0]);
        InputStream stream = new ByteArrayInputStream(certBytes);
        ks.load(stream, new char[0]);

        String alias = ks.aliases().nextElement();
        PrivateKey pk = (PrivateKey)ks.getKey(alias, new char[0]);

        Signature signature = Signature.getInstance("SHA256withRSA");
        signature.initSign(pk);
        signature.update(token.toString().getBytes(StandardCharsets.UTF_8));

        String signedPayload = Base64.getUrlEncoder().encodeToString(signature.sign());
        token.append(".");
        token.append(signedPayload);

        return token.toString();
    }
    private static void pushToAzure(Map<String, Object> cometdEvent) throws URISyntaxException, IOException, InterruptedException {
        Map<String, Object> payload =  (Map<String,Object>)cometdEvent.get("payload");
        Map<String, Object> header = ((Map<String,Object>)payload.get("ChangeEventHeader"));
        String recordType = header.get("entityName").toString();

        Object records = header.get("recordIds");
        String recordId = "";
        if(records.getClass().isArray()) {
            List<Object> recordList = Arrays.asList((Object[])records);
            recordId = recordList.get(0).toString();
        }

        EventGridEvent event = new EventGridEvent(
                UUID.randomUUID().toString(),
                "https://501software-dev-ed.my.salesforce.com/" + recordId,
                payload,
                recordType,
                DateTime.now(),
                "1.0"
            );

        List<EventGridEvent> events = new ArrayList<EventGridEvent>();
        events.add(event);
        eventGridClient.publishEvents(TopicEndpoint, events);
    }

}
