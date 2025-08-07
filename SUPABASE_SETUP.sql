-- ===============================================
-- Vault - Supabase Database Setup Script
-- ===============================================
-- Run this script in your Supabase SQL Editor

-- Enable necessary extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ===============================================
-- User Profiles Table
-- ===============================================
-- This extends the built-in auth.users table
CREATE TABLE IF NOT EXISTS public.user_profiles (
    id UUID REFERENCES auth.users(id) ON DELETE CASCADE PRIMARY KEY,
    username VARCHAR(50) UNIQUE NOT NULL,
    email VARCHAR(255) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT TIMEZONE('utc'::text, NOW()) NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT TIMEZONE('utc'::text, NOW()) NOT NULL
);

-- ===============================================
-- Secrets Table
-- ===============================================
CREATE TABLE IF NOT EXISTS public.secrets (
    id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
    user_id UUID REFERENCES auth.users(id) ON DELETE CASCADE NOT NULL,
    title VARCHAR(255) NOT NULL,
    content TEXT NOT NULL, -- This stores encrypted data
    secret_type VARCHAR(50) NOT NULL DEFAULT 'note',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT TIMEZONE('utc'::text, NOW()) NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT TIMEZONE('utc'::text, NOW()) NOT NULL
);

-- ===============================================
-- Enable Row Level Security (RLS)
-- ===============================================
ALTER TABLE public.user_profiles ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.secrets ENABLE ROW LEVEL SECURITY;

-- ===============================================
-- Row Level Security Policies for user_profiles
-- ===============================================
-- Users can only see and modify their own profile
CREATE POLICY "Users can view own profile" ON public.user_profiles
    FOR SELECT USING (auth.uid() = id);

CREATE POLICY "Users can update own profile" ON public.user_profiles
    FOR UPDATE USING (auth.uid() = id);

CREATE POLICY "Users can insert own profile" ON public.user_profiles
    FOR INSERT WITH CHECK (auth.uid() = id);

-- ===============================================
-- Row Level Security Policies for secrets
-- ===============================================
-- Users can only see and modify their own secrets
CREATE POLICY "Users can view own secrets" ON public.secrets
    FOR SELECT USING (auth.uid() = user_id);

CREATE POLICY "Users can create own secrets" ON public.secrets
    FOR INSERT WITH CHECK (auth.uid() = user_id);

CREATE POLICY "Users can update own secrets" ON public.secrets
    FOR UPDATE USING (auth.uid() = user_id);

CREATE POLICY "Users can delete own secrets" ON public.secrets
    FOR DELETE USING (auth.uid() = user_id);

-- ===============================================
-- Functions and Triggers
-- ===============================================
-- Function to automatically create user profile after registration
CREATE OR REPLACE FUNCTION public.handle_new_user()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO public.user_profiles (id, username, email)
    VALUES (
        NEW.id,
        COALESCE(NEW.raw_user_meta_data->>'username', split_part(NEW.email, '@', 1)),
        NEW.email
    );
    RETURN NEW;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Trigger to automatically create profile when user signs up
DROP TRIGGER IF EXISTS on_auth_user_created ON auth.users;
CREATE TRIGGER on_auth_user_created
    AFTER INSERT ON auth.users
    FOR EACH ROW EXECUTE FUNCTION public.handle_new_user();

-- Function to update updated_at timestamp
CREATE OR REPLACE FUNCTION public.update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Triggers to automatically update updated_at
CREATE TRIGGER update_user_profiles_updated_at
    BEFORE UPDATE ON public.user_profiles
    FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();

CREATE TRIGGER update_secrets_updated_at
    BEFORE UPDATE ON public.secrets
    FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();

-- ===============================================
-- Sample Data (Optional - for testing)
-- ===============================================
-- Uncomment these lines if you want some test data
-- INSERT INTO public.secrets (user_id, title, content, secret_type) VALUES
-- ('your-user-id-here', 'Test Note', 'This is a test encrypted note', 'note'),
-- ('your-user-id-here', 'Test Password', 'username:testuser|password:encrypted_password_here', 'password');

-- ===============================================
-- Verification Queries
-- ===============================================
-- Run these to verify everything was created correctly:
-- SELECT table_name FROM information_schema.tables WHERE table_schema = 'public';
-- SELECT * FROM public.user_profiles;
-- SELECT * FROM public.secrets;
