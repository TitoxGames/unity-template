# Tito Studio - Unity Framework

A modular Unity development framework for rapidly creating mobile and puzzle games.

---

# 🚀 Installation

Follow these **3 simple steps** to set up a new project.

## 1. Install Unity Installer

Open **Window → Package Manager**

Click **+ → Add package from Git URL**

```
https://github.com/TitoniumGames/unity-template.git?path=/Assets/Installer
```

```
git@github.com:TitoniumGames/unity-template.git?path=/Assets/Installer
```

Click **Add**.

---

## 2. Install Dependencies

Open the Installer window:

```
Tools
└── Game Template
    └── Framework Validator
```

Install all required dependencies.

### Automatically installed

- Newtonsoft.Json
- Addressables
- UniTask

### Manual installation

- Odin Inspector
- DOTween Pro

After importing DOTween:

- Generate **DOTween ASMDEF**
- Enable **UNITASK_DOTWEEN_SUPPORT**

When every dependency is marked as **Installed**, continue to the next step.

---

## 3. Install Unity Template

Open the Installer window again.

Click:

```
Install Unity Template
```

or install manually via Git URL:

```
https://github.com/TitoniumGames/unity-template.git?path=/Assets/TitoX
```

```
git@github.com:TitoniumGames/unity-template.git?path=/Assets/TitoX
```

---

# ✅ Done

Your Package Manager should now contain:

```
Packages - Tito Studio
├── Unity Installer
└── Unity Template
```

Your project is now ready for development.