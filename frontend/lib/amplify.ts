"use client";

import { Amplify } from 'aws-amplify';

export function configureAmplify() {
  Amplify.configure({
    Auth: {
      Cognito: {
        userPoolId: process.env.NEXT_PUBLIC_USER_POOL_ID!,
        userPoolClientId: process.env.NEXT_PUBLIC_USER_POOL_CLIENT_ID!,
        // You can add a `userPoolEndpoint` if you're using a custom domain for Cognito
      }
    },
    API: {
      REST: {
        ResumeAnalyzerApi: { // This is the API name
          endpoint: process.env.NEXT_PUBLIC_API_URL!,
          region: process.env.NEXT_PUBLIC_AWS_REGION!,
        }
      }
    }
  });
}
