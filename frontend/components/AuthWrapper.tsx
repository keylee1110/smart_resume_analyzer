"use client";

import { Authenticator, View, Image, Text, Heading, useTheme } from '@aws-amplify/ui-react';
import '@aws-amplify/ui-react/styles.css';
import { ReactNode } from 'react';
import { Sparkles } from 'lucide-react';

export default function AuthWrapper({ children }: { children: ReactNode }) {
    const formFields = {
        signIn: {
            username: {
                placeholder: 'Enter your Email',
                label: 'Email Address',
                isRequired: true,
            },
            password: {
                placeholder: 'Enter your Password',
                label: 'Password',
                isRequired: true,
            },
        },
        signUp: {
            email: {
                order: 1,
                placeholder: 'Enter your Email',
                label: 'Email Address',
                isRequired: true,
            },
            password: {
                order: 2,
                placeholder: 'Create a Password',
                label: 'Password',
                isRequired: true,
            },
            confirm_password: {
                order: 3,
                placeholder: 'Confirm Password',
                label: 'Confirm Password',
                isRequired: true,
            },
        },
    };

    return (
        <Authenticator.Provider>
            <Authenticator
                formFields={formFields}
                components={{
                    Header: () => (
                        <div className="flex flex-col items-center justify-center p-6 gap-4">
                            <div className="relative flex items-center justify-center w-16 h-16 rounded-2xl bg-gradient-to-tr from-primary to-accent shadow-[0_0_30px_rgba(139,92,246,0.5)]">
                                <Sparkles className="w-8 h-8 text-white" />
                            </div>
                            <div className="text-center">
                                <h1 className="text-3xl font-bold font-heading text-white tracking-tight">
                                    SmartResume
                                </h1>
                                <p className="text-muted-foreground mt-2">
                                    AI-Powered Career Acceleration
                                </p>
                            </div>
                        </div>
                    ),
                    Footer: () => (
                        <div className="text-center p-4 text-xs text-muted-foreground">
                            &copy; 2025 Smart Resume Analyzer. Secure Access.
                        </div>
                    )
                }}
                className="cyber-auth-container"
            >
                {({ signOut, user }) => (
                    <div className="relative min-h-screen">
                        {children}
                    </div>
                )}
            </Authenticator>
        </Authenticator.Provider>
    );
}
