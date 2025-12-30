# 強化学習 鬼ごっこ
Unity ML-Agentsを使った2エージェント対戦型の鬼ごっこゲームです。

![Animation1](https://github.com/user-attachments/assets/10fb3530-4cca-4f07-ae3c-968c7ab03411)


# 概要
- Tagger: 逃げる相手をタッチすると勝利
- Runner: 時間切れまで逃げ切れば勝利

# 技術スタック

- Unity 2022.3 LTS以上
- ML-Agents Release 21 (com.unity.ml-agents 3.0.0)
- Python 3.10.12
- mlagents 1.1.0

# 強化学習起動

PowerShellで起動

```shell
.\venv\Scripts\Activate
mlagents-learn config/tag_game.yaml --run-id=TagGame_v1
```

起動後Unityでplayを実行


# TensorBoardで進捗確認

PowerShellで起動

```shell
.\venv\Scripts\Activate
tensorboard --logdir results
```

http://localhost:6006 で開くと学習曲線が確認できる
