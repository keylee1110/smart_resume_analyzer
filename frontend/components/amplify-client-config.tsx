"use client";

import { Amplify } from 'aws-amplify';
import { awsConfig } from '@/lib/aws-config';
import { useEffect } from 'react';

export default function AmplifyClientConfig() {
  useEffect(() => {
    Amplify.configure(awsConfig);
  }, []);

  return null;
}
