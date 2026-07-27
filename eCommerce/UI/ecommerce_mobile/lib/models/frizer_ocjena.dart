/// Ocjena frizera od strane klijenta nakon odradjenog termina. Rucno pisan
/// model (bez json_serializable code-gen-a) da se izbjegne potreba za
/// build_runner - isti pristup kao [DostupnostDana].
class FrizerOcjena {
  final int? id;
  final int? terminId;
  final int? klijentId;
  final String? klijentImePrezime;
  final int? frizerId;
  final String? frizerImePrezime;
  final int? ocjena;
  final String? komentar;
  final DateTime? createdAt;

  FrizerOcjena({
    this.id,
    this.terminId,
    this.klijentId,
    this.klijentImePrezime,
    this.frizerId,
    this.frizerImePrezime,
    this.ocjena,
    this.komentar,
    this.createdAt,
  });

  factory FrizerOcjena.fromJson(Map<String, dynamic> json) => FrizerOcjena(
        id: json['id'] as int?,
        terminId: json['terminId'] as int?,
        klijentId: json['klijentId'] as int?,
        klijentImePrezime: json['klijentImePrezime'] as String?,
        frizerId: json['frizerId'] as int?,
        frizerImePrezime: json['frizerImePrezime'] as String?,
        ocjena: json['ocjena'] as int?,
        komentar: json['komentar'] as String?,
        createdAt: json['createdAt'] == null
            ? null
            : DateTime.tryParse(json['createdAt'].toString()),
      );

  Map<String, dynamic> toJson() => {
        'id': id,
        'terminId': terminId,
        'klijentId': klijentId,
        'frizerId': frizerId,
        'ocjena': ocjena,
        'komentar': komentar,
      };
}
